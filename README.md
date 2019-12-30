# Unity HTFramework AI

HTFramework的AI模块，必须依赖于HTFramework主框架使用。

## 环境

- Unity版本：2018.3.0及以上。

- .NET API版本：4.x。

- [百度AI开放平台](https://ai.baidu.com/)。

- [HTFramework](https://github.com/SaiTingHu/HTFramework)。

## 模块简介

- [Pathfinding](https://wanderer.blog.csdn.net/article/details/103761142) - 常规的A*寻路算法，目前支持两点间寻路，或只提供起点后根据行走值寻所有可行走节点。

- [Speech](https://wanderer.blog.csdn.net/article/details/103764141) - 对接百度AI开放平台，封装的语音技术接口。

- [OCR](https://wanderer.blog.csdn.net/article/details/103765003) - 对接百度AI开放平台，封装的文字识别接口。

## 使用方法

- 1.拉取框架到项目中的Assets文件夹下（Assets/HTFramework/）。

- 2.将框架根目录下的HTFramework.prefab拖到主场景，并删除主场景其他的东西（除了框架的主要模块，其他任何东西都应该是动态加载的）。

- 3.拉取本模块到项目中的Assets文件夹下（Assets/HTFrameworkAI/）。

- 4.开始开发。
