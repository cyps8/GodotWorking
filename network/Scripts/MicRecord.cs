using Godot;
using System;

public class MicRecord : Control
{
    //private AudioEffectRecord _effect;
    //private AudioStreamSample _recording;

    //byte[] recordedData;

    //float timer;

    // public override void _Ready()
    // {
    //     timer = 0;
    //     int idx = AudioServer.GetBusIndex("Record");
    //     _effect = (AudioEffectRecord)AudioServer.GetBusEffect(idx, 0);
    // }

    // public override void _Process(float dt)
    // {
    //     if (_effect.IsRecordingActive())
    //     {
    //         if (_effect.GetRecording() != null)
    //         {
    //             _recording = _effect.GetRecording();
    //             //_effect.SetRecordingActive(false);
    //             //_effect.SetRecordingActive(true);
    //             _recording.Format = AudioStreamSample.FormatEnum.Format16Bits;
    //         }
    //     }

    //     if (Input.IsActionPressed("voice"))
    //     {
    //         if (!_effect.IsRecordingActive())
    //         _effect.SetRecordingActive(true);

    //         timer += dt;

    //         if (timer > 0.05f)
    //         {
    //             // if (recordedData != null)
    //             // {
    //                 if(SceneManager.isServer == true)
    //                 {
    //                     //DataManager.Send.ServerVoiceChat(recordedData);
    //                     //GameManager.PlayVoice(recordedData);
    //                     recordedData = Read16BitSamples(_recording);
    //                     _recording = Write16BitSamples(recordedData);
    //                     GameManager.PlayVoice2(_recording);
    //                 }
    //                 else
    //                 {
    //                     DataManager.Send.ClientVoiceChat(recordedData);
    //                 }
    //                 //recordedData = null;
    //             //}
    //             timer -= 0.05f;
    //         }
    //     }
    //     else
    //     {
    //         if (_effect.IsRecordingActive())
    //         _effect.SetRecordingActive(false);
    //     }
    // }

    // public AudioStreamSample Write16BitSamples(byte[] samples)
    // {
    //     byte[] bytes = new byte[samples.Length * 2];
    //     for (int i = 0; i < samples.Length; i++)
    //     {
    //         int j = i * 2;
    //         int u = (samples[i] * 32768) + 32768;
    //         // Emulate cast from unsigned to signed
    //         u = (u - 32768) & 0xffff;
    //         // Assign low and high byte
    //         bytes[j] = ((byte)(u & 0xff));
    //         bytes[j + 1] = ((byte)(u >> 8));
    //     }
    //     AudioStreamSample stream = new AudioStreamSample();
    //     stream.Stereo = false;
    //     stream.Format = AudioStreamSample.FormatEnum.Format16Bits;
    //     stream.Data = bytes;
    //     GD.Print($"out: {samples.Length}");
    //     return stream;
    // }

    // public byte[] Read16BitSamples(AudioStreamSample stream)
    // {
    //     byte[] bytes = stream.Data;
    //     GD.Print($"in: {bytes.Length}");
    //     byte[] samples = new byte[bytes.Length / 2];
    //     int i = 0;
    //     int j = 0;
    //     // Read by packs of 2 bytes
    //     while (i < bytes.Length)
    //     {
    //         byte b0 = bytes[i];
    //         byte b1 = bytes[i + 1];
    //         // Combine low bits and high bits to obtain 16-bit value
    //         var u = b0 | (b1 << 8);
    //         // Emulate signed to unsigned 16-bit conversion
    //         u = (u + 32768) & 0xffff;
    //         // Convert to -1..1 range
    //         byte s = ((byte)((u - 32768) / 32768));
    //         samples[j] = s;
    //         i += 2;
    //         j++;
    //     }
    //     return samples;
    // }
}
