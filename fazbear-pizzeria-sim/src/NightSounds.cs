using System.Media;

namespace FazbearPizzeria;

/// <summary>Small original synth cues for the prototype; keeps the game independent of ripped audio files.</summary>
internal static class NightSounds
{
    private const int SampleRate = 22050;
    private static readonly Dictionary<string, SoundPlayer> Players = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object Sync = new();

    public static void Play(string cue)
    {
        try
        {
            lock (Sync)
            {
                if (!Players.TryGetValue(cue, out var player))
                {
                    var stream = new MemoryStream(BuildWave(cue));
                    player = new SoundPlayer(stream);
                    player.Load();
                    Players[cue] = player;
                }
                player.Play();
            }
        }
        catch (Exception) { /* Audio is optional; unsupported devices must not interrupt the night. */ }
    }

    private static byte[] BuildWave(string cue)
    {
        var duration = cue switch { "jumpscare" => .62, "approach" => .2, "monitor-up" or "monitor-down" => .28, _ => .14 };
        var sampleCount = (int)(SampleRate * duration);
        using var stream = new MemoryStream(44 + sampleCount * 2);
        using var writer = new BinaryWriter(stream);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + sampleCount * 2);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1);
        writer.Write((short)1); writer.Write(SampleRate); writer.Write(SampleRate * 2); writer.Write((short)2); writer.Write((short)16);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(sampleCount * 2);
        var random = new Random(cue.GetHashCode(StringComparison.Ordinal));
        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (double)SampleRate;
            var phase = t / duration;
            var envelope = Math.Min(1, t * 90) * Math.Min(1, (duration - t) * 20);
            double sample = cue switch
            {
                "monitor-up" => Tone(t, phase < .55 ? 430 : 760, .12) + Tone(t, phase >= .55 ? 980 : 0, .1),
                "monitor-down" => Tone(t, phase < .5 ? 780 : 390, .11),
                "door" => Tone(t, 95 + 35 * (1 - phase), .34) + Tone(t, 165, .12),
                "light" => Tone(t, 1450, .18) * (phase < .36 ? 1 : 0),
                "warning" => Tone(t, 1120, .22) * (Math.Sin(phase * Math.PI * 8) > 0 ? 1 : 0),
                "approach" => Tone(t, 72 + 55 * phase, .25) + Tone(t, 130 + 90 * phase, .12),
                "task" => Tone(t, 900 + 300 * phase, .13) * (phase < .58 ? 1 : 0),
                "jumpscare" => Tone(t, 110 + phase * 740, .28) + (random.NextDouble() * 2 - 1) * .26,
                _ => Tone(t, 650, .12)
            } * envelope;
            writer.Write((short)Math.Clamp((int)(sample * short.MaxValue), short.MinValue, short.MaxValue));
        }
        writer.Flush();
        return stream.ToArray();
    }

    private static double Tone(double time, double frequency, double volume) =>
        frequency <= 0 ? 0 : Math.Sin(2 * Math.PI * frequency * time) * volume;
}
