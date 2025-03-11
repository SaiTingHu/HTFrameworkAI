using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HT.Framework.AI
{
    /// <summary>
    /// https://pollinations.ai/
    /// </summary>
    public static class PollinationsAI
    {
        /// <summary>
        /// 获取图像生成的可用大模型列表
        /// </summary>
        public static async Task<List<string>> GetImageGenerationModels()
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);

            try
            {
                string url = "https://image.pollinations.ai/models";
                using HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                List<string> models = new List<string>();
                JsonData jsonData = JsonToolkit.StringToJson(result);
                if (jsonData != null)
                {
                    for (int i = 0; i < jsonData.Count; i++)
                    {
                        models.Add(jsonData[i].ToString());
                    }
                }
                return models;
            }
            catch (Exception e)
            {
                Log.Error("请求出错：" + e.Message);
                return null;
            }
        }
        /// <summary>
        /// 图像生成
        /// </summary>
        /// <param name="prompt">需生成的图像描述</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="seed">生成种子</param>
        /// <param name="model">大模型</param>
        /// <param name="nologo">去掉LOGO</param>
        /// <returns>图像</returns>
        public static async Task<byte[]> ImageGeneration(string prompt, int width, int height, int seed, string model = "flux", bool nologo = true)
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);

            try
            {
                string url = $"https://image.pollinations.ai/prompt/{prompt}?width={width}&height={height}&seed={seed}&model={model}&nologo={nologo}";
                using HttpResponseMessage response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();

                byte[] result = await response.Content.ReadAsByteArrayAsync();
                return result;
            }
            catch (Exception e)
            {
                Log.Error("请求出错：" + e.Message);
                return null;
            }
        }
    }
}