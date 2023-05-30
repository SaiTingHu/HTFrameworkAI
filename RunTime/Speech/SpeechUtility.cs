using UnityEngine;
using System.Text;
using System.IO;
using System;

namespace HT.Framework.AI
{
    /// <summary>
    /// 语音管理实用工具
    /// </summary>
    public static class SpeechUtility
    {
        #region Calculation Method
        private const int BlockSize_16Bit = 2;

        private static float[] Convert8BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            if (wavSize <= 0 || wavSize != dataSize)
            {
                Log.Error("转换AudioClip失败：未获取到有效的WAV字节数据！");
                return null;
            }

            float[] data = new float[wavSize];
            sbyte maxValue = sbyte.MaxValue;
            int i = 0;
            while (i < wavSize)
            {
                data[i] = (float)source[i] / maxValue;
                i += 1;
            }
            return data;
        }
        private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            if (wavSize <= 0 || wavSize != dataSize)
            {
                Log.Error("转换AudioClip失败：未获取到有效的WAV字节数据！");
                return null;
            }

            int x = sizeof(short);
            int convertedSize = wavSize / x;

            float[] data = new float[convertedSize];
            short maxValue = short.MaxValue;
            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
                i += 1;
            }
            return data;
        }
        private static float[] Convert24BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            if (wavSize <= 0 || wavSize != dataSize)
            {
                Log.Error("转换AudioClip失败：未获取到有效的WAV字节数据！");
                return null;
            }

            int x = 3;
            int convertedSize = wavSize / x;
            
            float[] data = new float[convertedSize];
            int maxValue = int.MaxValue;
            byte[] block = new byte[sizeof(int)];
            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                Buffer.BlockCopy(source, offset, block, 1, x);
                data[i] = (float)BitConverter.ToInt32(block, 0) / maxValue;
                i += 1;
            }
            return data;
        }
        private static float[] Convert32BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = BitConverter.ToInt32(source, headerOffset);
            headerOffset += sizeof(int);
            if (wavSize <= 0 || wavSize != dataSize)
            {
                Log.Error("转换AudioClip失败：未获取到有效的WAV字节数据！");
                return null;
            }

            int x = sizeof(float);
            int convertedSize = wavSize / x;
            
            float[] data = new float[convertedSize];
            int maxValue = int.MaxValue;
            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt32(source, offset) / maxValue;
                i += 1;
            }
            return data;
        }
        private static int WriteFileHeader(ref MemoryStream stream, int fileSize)
        {
            int count = 0;
            int total = 12;

            byte[] riff = Encoding.ASCII.GetBytes("RIFF");
            count += WriteBytesToMemoryStream(ref stream, riff);

            int chunkSize = fileSize - 8;
            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(chunkSize));

            byte[] wave = Encoding.ASCII.GetBytes("WAVE");
            count += WriteBytesToMemoryStream(ref stream, wave);

            if (count != total)
            {
                Log.Error("转换字节数组失败：写入的字节数组长度异常！");
            }
            return count;
        }
        private static int WriteFileFormat(ref MemoryStream stream, int channels, int sampleRate, ushort bitDepth)
        {
            int count = 0;
            int total = 24;

            byte[] id = Encoding.ASCII.GetBytes("fmt ");
            count += WriteBytesToMemoryStream(ref stream, id);

            int subchunk1Size = 16;
            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(subchunk1Size));

            ushort audioFormat = 1;
            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(audioFormat));

            ushort numChannels = Convert.ToUInt16(channels);
            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(numChannels));

            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(sampleRate));

            int byteRate = sampleRate * channels * BytesPerSample(bitDepth);
            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(byteRate));

            ushort blockAlign = Convert.ToUInt16(channels * BytesPerSample(bitDepth));
            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(blockAlign));

            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(bitDepth));

            if (count != total)
            {
                Log.Error("转换字节数组失败：写入的字节数组长度异常！");
            }
            return count;
        }
        private static int WriteFileData(ref MemoryStream stream, AudioClip audioClip, ushort bitDepth)
        {
            int count = 0;
            int total = 8;

            float[] data = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(data, 0);

            byte[] bytes = ConvertAudioClipDataToInt16ByteArray(data);

            byte[] id = Encoding.ASCII.GetBytes("data");
            count += WriteBytesToMemoryStream(ref stream, id);

            int subchunk2Size = Convert.ToInt32(audioClip.samples * BlockSize_16Bit);
            count += WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(subchunk2Size));

            if (count != total)
            {
                Log.Error("转换字节数组失败：写入的字节数组长度异常！");
            }

            count += WriteBytesToMemoryStream(ref stream, bytes);

            if (bytes.Length != subchunk2Size)
            {
                Log.Error("转换字节数组失败：写入的字节数组长度异常！");
            }
            return count;
        }
        private static int WriteBytesToMemoryStream(ref MemoryStream stream, byte[] bytes)
        {
            int count = bytes.Length;
            stream.Write(bytes, 0, count);
            return count;
        }
        private static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
        {
            MemoryStream dataStream = new MemoryStream();

            int x = sizeof(short);

            short maxValue = short.MaxValue;

            int i = 0;
            while (i < data.Length)
            {
                dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
                i += 1;
            }
            byte[] bytes = dataStream.ToArray();

            if (data.Length * x != bytes.Length)
            {
                Log.Error("转换字节数组失败：写入的字节数组长度异常！");
            }

            dataStream.Close();

            return bytes;
        }

        private static int BytesPerSample(ushort bitDepth)
        {
            return bitDepth / 8;
        }
        private static string FormatCode(ushort code)
        {
            switch (code)
            {
                case 1:
                    return "PCM";
                case 2:
                    return "ADPCM";
                case 3:
                    return "IEEE";
                case 6:
                    return "a-law";
                case 7:
                    return "μ-law";
                case 49:
                    return "GSM";
                case 65534:
                    return "WaveFormatExtensable";
                default:
                    Log.Warning("未知的WAV格式代码:" + code);
                    return "";
            }
        }
        #endregion

        /// <summary>
        /// 将WAV音频格式文件转换为AudioClip
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>转换完成的AudioClip</returns>
        public static AudioClip ToAudioClip(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Error("加载WAV文件失败：未找到 " + filePath + " 文件！");
                return null;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return ToAudioClip(fileBytes, 0);
        }
        /// <summary>
        /// 将字节数组转换为AudioClip
        /// </summary>
        /// <param name="fileBytes">文件的字节数组</param>
        /// <param name="offsetSamples">采样偏移</param>
        /// <param name="name">名称</param>
        /// <returns>转换完成的AudioClip</returns>
        public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
        {
            int subchunk1Size = BitConverter.ToInt32(fileBytes, 16);
            ushort audioFormatCode = BitConverter.ToUInt16(fileBytes, 20);
            string audioFormat = FormatCode(audioFormatCode);

            if (audioFormatCode != 1 && audioFormatCode != 65534)
            {
                Log.Error("转换AudioClip失败：当前仅支持转换PCM或WAV的扩展格式！");
                return null;
            }

            ushort channels = BitConverter.ToUInt16(fileBytes, 22);
            int sampleRate = BitConverter.ToInt32(fileBytes, 24);
            ushort bitsPerSample = BitConverter.ToUInt16(fileBytes, 34);
            int headerOffset = 16 + 4 + subchunk1Size + 4;
            int subchunk2Size = BitConverter.ToInt32(fileBytes, headerOffset);

            float[] data;
            switch (bitsPerSample)
            {
                case 8:
                    data = Convert8BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2Size);
                    break;
                case 16:
                    data = Convert16BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2Size);
                    break;
                case 24:
                    data = Convert24BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2Size);
                    break;
                case 32:
                    data = Convert32BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2Size);
                    break;
                default:
                    Log.Error("转换AudioClip失败：不支持 " + bitsPerSample + " 量化位数！");
                    return null;
            }

            if (data != null)
            {
                AudioClip audioClip = AudioClip.Create(name, data.Length, channels, sampleRate, false);
                audioClip.SetData(data, 0);
                return audioClip;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 将AudioClip转换为字节数组
        /// </summary>
        /// <param name="audioClip">AudioClip对象</param>
        /// <returns>转换完成的字节数组</returns>
        public static byte[] FromAudioClip(AudioClip audioClip)
        {
            return FromAudioClip(audioClip, false, string.Empty);
        }
        /// <summary>
        /// 将AudioClip转换为字节数组
        /// </summary>
        /// <param name="audioClip">AudioClip对象</param>
        /// <param name="saveAsFile">是否保存为文件</param>
        /// <param name="filepath">保存路径</param>
        /// <returns>转换完成的字节数组</returns>
        public static byte[] FromAudioClip(AudioClip audioClip, bool saveAsFile, string filepath)
        {
            MemoryStream stream = new MemoryStream();

            int headerSize = 44;
            ushort bitDepth = 16;
            int fileSize = audioClip.samples * BlockSize_16Bit + headerSize;

            WriteFileHeader(ref stream, fileSize);
            WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
            WriteFileData(ref stream, audioClip, bitDepth);

            byte[] bytes = stream.ToArray();
            if (bytes.Length != fileSize)
            {
                Log.Error("转换字节数组失败：转换完成后的字节数组长度异常！");
                return null;
            }

            if (saveAsFile)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                File.WriteAllBytes(filepath, bytes);
            }

            stream.Close();
            return bytes;
        }
    }
}