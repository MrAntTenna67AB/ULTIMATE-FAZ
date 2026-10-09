using System.Media;

namespace FazbearPizzeria;

internal static class NightSounds
{
    private sealed record LoadedSound(SoundPlayer Player, MemoryStream Stream);

    private static readonly Dictionary<string, string> Effects = new(StringComparer.OrdinalIgnoreCase)
    {
        ["monitor-up"] = "CAMERA_VIDEO_LOA_60105303.wav",
        ["monitor-down"] = "put down monitor.wav",
        ["door"] = "door closing.wav",
        ["light"] = "door lights on sound.wav",
        ["light-spotted"] = "animatronic spotted at the lights.wav",
        ["camera"] = "on a cam.wav",
        ["movement"] = "animatronic moving from a cam.wav",
        ["warning"] = "animatronic moving from a cam.wav",
        ["task"] = "blip3.wav",
        ["jumpscare"] = "fnaf one jumpscare.wav",
        ["golden-jumpscare"] = "fnaf one golden freddy jumpscare.wav",
        ["six-am"] = "when hitting 6 am (plays beffore the winning a night one).wav",
        ["win"] = "winning a night.wav"
    };

    private static readonly Dictionary<string, LoadedSound> Players = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<LoadedSound> AmbiencePlayers = [];
    private static readonly object Sync = new();
    private static string SoundDirectory => Path.Combine(AppContext.BaseDirectory, "sound-bank");
    private static LoadedSound? _menuTheme;
    private static System.Threading.Timer? _victoryTimer;

    public static void StartMenuMusic()
    {
        lock (Sync)
        {
            _victoryTimer?.Dispose();
            _victoryTimer = null;
            StopMenuMusicCore();
            try
            {
                _menuTheme = Load("main menu theme.wav", 1);
                _menuTheme?.Player.PlayLooping();
            }
            catch (Exception) { _menuTheme = null; }
        }
    }

    public static void StopMenuMusic()
    {
        lock (Sync)
        {
            StopMenuMusicCore();
            _victoryTimer?.Dispose();
            _victoryTimer = null;
        }
    }

    private static void StopMenuMusicCore()
    {
        try { _menuTheme?.Player.Stop(); } catch (Exception) { }
        _menuTheme = null;
    }

    public static void Play(string cue)
    {
        try
        {
            lock (Sync)
            {
                if (!Effects.TryGetValue(cue, out var file)) return;
                var sound = Load(file, 1);
                sound?.Player.Play();
            }
        }
        catch (Exception) { /* Optional audio must never interrupt the game. */ }
    }

    public static void PlayVictorySequence()
    {
        StopMenuMusic();
        Play("six-am");
        var path = Path.Combine(SoundDirectory, Effects["six-am"]);
        var durationMs = Math.Clamp((int)Math.Ceiling(GetWaveDurationSeconds(path) * 1000), 1000, 30000);
        lock (Sync)
        {
            _victoryTimer?.Dispose();
            _victoryTimer = new System.Threading.Timer(_ => Play("win"), null, durationMs, System.Threading.Timeout.Infinite);
        }
    }

    private static double GetWaveDurationSeconds(string path)
    {
        try
        {
            var wave = File.ReadAllBytes(path);
            if (wave.Length < 44) return 2;
            var byteRate = BitConverter.ToInt32(wave, 28);
            for (var offset = 12; offset + 8 <= wave.Length;)
            {
                var chunk = System.Text.Encoding.ASCII.GetString(wave, offset, 4);
                var size = BitConverter.ToInt32(wave, offset + 4);
                if (size < 0 || offset + 8L + size > wave.Length) return 2;
                if (chunk == "data" && byteRate > 0) return (double)size / byteRate;
                offset += 8 + size + (size & 1);
            }
        }
        catch (Exception) { }
        return 2;
    }

    public static void StartAmbience()
    {
        StopMenuMusic();
        lock (Sync)
        {
            _victoryTimer?.Dispose();
            _victoryTimer = null;
        }
        lock (Sync)
        {
            StopAmbienceCore();
            StartLoop("office ambience2.wav", .30);
            StartLoop("Buzz_Fan_Florescent2 (plays at a low sound in the office).wav", .12);
        }
    }

    public static void StopAmbience()
    {
        lock (Sync) StopAmbienceCore();
    }

    private static void StartLoop(string file, double gain)
    {
        try
        {
            var sound = Load(file, gain);
            if (sound is null) return;
            sound.Player.PlayLooping();
            AmbiencePlayers.Add(sound);
        }
        catch (Exception) { }
    }

    private static void StopAmbienceCore()
    {
        foreach (var sound in AmbiencePlayers)
        {
            try { sound.Player.Stop(); } catch (Exception) { }
        }
        AmbiencePlayers.Clear();
    }

    private static LoadedSound? Load(string file, double gain)
    {
        var key = file + "|" + gain.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (Players.TryGetValue(key, out var loaded)) return loaded;
        var path = Path.Combine(SoundDirectory, file);
        if (!File.Exists(path)) return null;
        var bytes = File.ReadAllBytes(path);
        if (gain < 1) ApplyPcmGain(bytes, gain);
        var stream = new MemoryStream(bytes, writable: false);
        var player = new SoundPlayer(stream);
        player.Load();
        loaded = new LoadedSound(player, stream);
        Players[key] = loaded;
        return loaded;
    }

    private static void ApplyPcmGain(byte[] wave, double gain)
    {
        if (wave.Length < 44 || System.Text.Encoding.ASCII.GetString(wave, 0, 4) != "RIFF") return;
        var format = 0;
        var bits = 0;
        var dataOffset = -1;
        var dataLength = 0;
        for (var offset = 12; offset + 8 <= wave.Length;)
        {
            var chunk = System.Text.Encoding.ASCII.GetString(wave, offset, 4);
            var size = BitConverter.ToInt32(wave, offset + 4);
            if (size < 0 || offset + 8L + size > wave.Length) return;
            if (chunk == "fmt " && size >= 16)
            {
                format = BitConverter.ToInt16(wave, offset + 8);
                bits = BitConverter.ToInt16(wave, offset + 22);
            }
            else if (chunk == "data")
            {
                dataOffset = offset + 8;
                dataLength = size;
                break;
            }
            offset += 8 + size + (size & 1);
        }
        if (format != 1 || dataOffset < 0) return;
        if (bits == 16)
        {
            var end = dataOffset + dataLength - 1;
            for (var i = dataOffset; i < end; i += 2)
            {
                var sample = BitConverter.ToInt16(wave, i);
                var scaled = (short)Math.Clamp((int)Math.Round(sample * gain), short.MinValue, short.MaxValue);
                var sampleBytes = BitConverter.GetBytes(scaled);
                wave[i] = sampleBytes[0]; wave[i + 1] = sampleBytes[1];
            }
        }
        else if (bits == 8)
        {
            var end = dataOffset + dataLength;
            for (var i = dataOffset; i < end; i++)
                wave[i] = (byte)Math.Clamp((int)Math.Round(128 + (wave[i] - 128) * gain), 0, 255);
        }
    }
}
