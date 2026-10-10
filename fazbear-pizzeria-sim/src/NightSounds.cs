using System.Media;
using NVorbis;

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
        ["pee-monster-jumpscare"] = "pee monster jumpscare.wav",
        ["charlie-kirk-jumpscare"] = "we are charlie kirk.ogg",
        ["six-am"] = "when hitting 6 am (plays beffore the winning a night one).wav",
        ["win"] = "winning a night.wav"
    };

    private static readonly Dictionary<string, LoadedSound> Players = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<LoadedSound> AmbiencePlayers = [];
    private static readonly object Sync = new();
    private static string SoundDirectory => Path.Combine(AppContext.BaseDirectory, "sound-bank");
    private static LoadedSound? _menuTheme;
    private static string? _activeThemeFile;
    private static int _musicVolume = 100;
    private static int _effectsVolume = 100;
    private static int _activeThemeVolume = -1;
    private static System.Threading.Timer? _victoryTimer;
    private static System.Threading.Timer? _ambienceResumeTimer;
    private static LoadedSound? _activeEffect;
    private static string? _activeEffectCue;
    private static bool _ambienceRequested;
    private static int _effectGeneration;

    public static void SetVolumes(int music, int effects)
    {
        lock (Sync)
        {
            _musicVolume = Math.Clamp(music, 0, 100);
            _effectsVolume = Math.Clamp(effects, 0, 100);
        }
    }

    public static void StartMenuMusic()
    {
        StartTheme("main menu theme.wav");
    }

    public static void StartTheme(string file)
    {
        lock (Sync)
        {
            if (_activeThemeFile?.Equals(file, StringComparison.OrdinalIgnoreCase) == true && _activeThemeVolume == _musicVolume && _menuTheme is not null) return;
            _victoryTimer?.Dispose();
            _victoryTimer = null;
            StopMenuMusicCore();
            try
            {
                _menuTheme = Load(file, _musicVolume / 100.0);
                _activeThemeFile = _menuTheme is null ? null : file;
                _activeThemeVolume = _menuTheme is null ? -1 : _musicVolume;
                _menuTheme?.Player.PlayLooping();
            }
            catch (Exception) { _menuTheme = null; _activeThemeFile = null; _activeThemeVolume = -1; }
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
        _activeThemeFile = null;
        _activeThemeVolume = -1;
    }

    public static void Play(string cue)
    {
        try
        {
            lock (Sync)
            {
                if (!Effects.TryGetValue(cue, out var file)) return;
                var sound = Load(file, _effectsVolume / 100.0);
                if (sound is null) return;
                _ambienceResumeTimer?.Dispose();
                _ambienceResumeTimer = null;
                sound.Player.Play();
                _activeEffect = sound;
                _activeEffectCue = cue;
                var generation = ++_effectGeneration;
                if (_ambienceRequested)
                {
                    var durationMs = Math.Clamp((int)Math.Ceiling(GetWaveDurationSeconds(Path.Combine(SoundDirectory, file)) * 1000), 150, 30000);
                    _ambienceResumeTimer = new System.Threading.Timer(_ => ResumeSoundscape(generation), null, durationMs, System.Threading.Timeout.Infinite);
                }
            }
        }
        catch (Exception) { /* Optional audio must never interrupt the game. */ }
    }

    public static void StopDoorLightSound()
    {
        lock (Sync)
        {
            if (_activeEffectCue is not ("light" or "light-spotted")) return;
            _ambienceResumeTimer?.Dispose();
            _ambienceResumeTimer = null;
            ++_effectGeneration;
            try { _activeEffect?.Player.Stop(); } catch (Exception) { }
            _activeEffect = null;
            _activeEffectCue = null;
            if (_ambienceRequested) StartAmbienceCore();
        }
    }

    private static void ResumeSoundscape(int generation)
    {
        lock (Sync)
        {
            if (generation != _effectGeneration || !_ambienceRequested) return;
            _ambienceResumeTimer?.Dispose();
            _ambienceResumeTimer = null;
            _activeEffect = null;
            _activeEffectCue = null;
            StartAmbienceCore();
        }
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
            _ambienceRequested = true;
            _ambienceResumeTimer?.Dispose();
            _ambienceResumeTimer = null;
            ++_effectGeneration;
            StartAmbienceCore();
        }
    }

    public static void StopAmbience()
    {
        lock (Sync)
        {
            _ambienceRequested = false;
            _ambienceResumeTimer?.Dispose();
            _ambienceResumeTimer = null;
            ++_effectGeneration;
            try { _activeEffect?.Player.Stop(); } catch (Exception) { }
            _activeEffect = null;
            _activeEffectCue = null;
            StopAmbienceCore();
        }
    }

    private static void StartAmbienceCore()
    {
        StopAmbienceCore();
        var officePath = Path.Combine(SoundDirectory, "office ambience2.wav");
        var fanPath = Path.Combine(SoundDirectory, "Buzz_Fan_Florescent2 (plays at a low sound in the office).wav");
        LoadedSound? mixed = null;
        try
        {
            if (File.Exists(officePath) && File.Exists(fanPath))
            {
                var gain = _effectsVolume / 100.0;
                var wave = MixAmbience(File.ReadAllBytes(officePath), File.ReadAllBytes(fanPath), .30 * gain, .12 * gain);
                var stream = new MemoryStream(wave, writable: false);
                mixed = new LoadedSound(new SoundPlayer(stream), stream);
                mixed.Player.Load();
            }
        }
        catch (Exception) { mixed = null; }
        if (mixed is not null)
        {
            try { mixed.Player.PlayLooping(); AmbiencePlayers.Add(mixed); return; }
            catch (Exception) { }
        }
        StartLoop("office ambience2.wav", .30 * _effectsVolume / 100.0);
    }

    private static byte[] MixAmbience(byte[] office, byte[] fan, double officeGain, double fanGain)
    {
        static (short format, short channels, int rate, short bits, byte[] fmt, byte[] data) Read(byte[] wav)
        {
            if (wav.Length < 44 || System.Text.Encoding.ASCII.GetString(wav, 0, 4) != "RIFF") throw new InvalidDataException();
            short format = 0, channels = 0, bits = 0; var rate = 0; byte[]? fmt = null; byte[]? data = null;
            for (var offset = 12; offset + 8 <= wav.Length;)
            {
                var chunk = System.Text.Encoding.ASCII.GetString(wav, offset, 4);
                var size = BitConverter.ToInt32(wav, offset + 4);
                if (size < 0 || offset + 8L + size > wav.Length) throw new InvalidDataException();
                if (chunk == "fmt " && size >= 16)
                {
                    format = BitConverter.ToInt16(wav, offset + 8); channels = BitConverter.ToInt16(wav, offset + 10);
                    rate = BitConverter.ToInt32(wav, offset + 12); bits = BitConverter.ToInt16(wav, offset + 22);
                    fmt = wav.AsSpan(offset + 8, size).ToArray();
                }
                else if (chunk == "data") data = wav.AsSpan(offset + 8, size).ToArray();
                offset += 8 + size + (size & 1);
            }
            if (format != 1 || fmt is null || data is null) throw new InvalidDataException();
            return (format, channels, rate, bits, fmt, data);
        }

        var a = Read(office); var b = Read(fan);
        if (a.channels != b.channels || a.rate != b.rate || a.bits != b.bits || a.bits is not (8 or 16)) throw new InvalidDataException();
        var bytesPerSample = a.bits / 8; var maxSamples = Math.Max(a.data.Length, b.data.Length) / bytesPerSample;
        var data = new byte[maxSamples * bytesPerSample];
        var samplesA = a.data.Length / bytesPerSample; var samplesB = b.data.Length / bytesPerSample;
        for (var i = 0; i < maxSamples; i++)
        {
            var ia = i % samplesA * bytesPerSample; var ib = i % samplesB * bytesPerSample;
            if (a.bits == 16)
            {
                var value = BitConverter.ToInt16(a.data, ia) * officeGain + BitConverter.ToInt16(b.data, ib) * fanGain;
                BitConverter.GetBytes((short)Math.Clamp((int)Math.Round(value), short.MinValue, short.MaxValue)).CopyTo(data, i * 2);
            }
            else
            {
                var value = 128 + (a.data[ia] - 128) * officeGain + (b.data[ib] - 128) * fanGain;
                data[i] = (byte)Math.Clamp((int)Math.Round(value), 0, 255);
            }
        }
        using var output = new MemoryStream(); using var writer = new BinaryWriter(output);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + a.fmt.Length + data.Length); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt ")); writer.Write(a.fmt.Length); writer.Write(a.fmt);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(data.Length); writer.Write(data);
        if ((data.Length & 1) != 0) writer.Write((byte)0);
        var result = output.ToArray(); BitConverter.GetBytes(result.Length - 8).CopyTo(result, 4); return result;
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
        if (bytes.AsSpan().StartsWith("OggS"u8)) bytes = DecodeOggToWave(bytes, gain);
        else if (gain < 1) ApplyPcmGain(bytes, gain);
        var stream = new MemoryStream(bytes, writable: false);
        var player = new SoundPlayer(stream);
        player.Load();
        loaded = new LoadedSound(player, stream);
        Players[key] = loaded;
        return loaded;
    }

    private static byte[] DecodeOggToWave(byte[] ogg, double gain)
    {
        using var input = new MemoryStream(ogg, writable: false);
        using var decoder = new VorbisReader(input, closeOnDispose: false);
        var channels = decoder.Channels;
        var sampleRate = decoder.SampleRate;
        if (channels is < 1 or > 2 || sampleRate < 8000) throw new InvalidDataException("Unsupported OGG audio format.");

        using var pcm = new MemoryStream();
        using (var writer = new BinaryWriter(pcm, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            var samples = new float[8192];
            int count;
            while ((count = decoder.ReadSamples(samples, 0, samples.Length)) > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var value = Math.Clamp(samples[i] * gain, -1f, 1f);
                    writer.Write((short)Math.Clamp((int)Math.Round(value * (value < 0 ? 32768 : 32767)), short.MinValue, short.MaxValue));
                }
            }
        }

        using var wave = new MemoryStream(44 + (int)pcm.Length);
        using (var writer = new BinaryWriter(wave, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + (int)pcm.Length);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1);
            writer.Write((short)channels); writer.Write(sampleRate); writer.Write(sampleRate * channels * 2);
            writer.Write((short)(channels * 2)); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write((int)pcm.Length);
            pcm.Position = 0; pcm.CopyTo(wave);
        }
        return wave.ToArray();
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
