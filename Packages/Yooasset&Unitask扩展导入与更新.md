## Yooasset版本

[Releases · tuyoogame/YooAsset (github.com)](https://github.com/tuyoogame/YooAsset)

## Yooasset导入Unity或更新

- [Releases · tuyoogame/YooAsset (github.com)](https://github.com/tuyoogame/YooAsset/releases)

- 将仓库拷贝到项目的`Packages/`下

### UniTask 扩展 导入更新

- 具体参考`YooAsset\Samples~\UniTask Sample\README.md`
- 创建\`package.json`填写: 
- ```
  {
      "name": "com.tuyoogame.yooasset.unitask",
      "displayName": "UniTask.YooAsset",
      "version": "2.0.3-preview",
      "unity": "2019.4",
      "description": "UniTask.YooAsset",
      "author": {
          "name": "TuYoo Games",
          "url": "https://github.com/tuyoogame/YooAsset"
      },
      "repository": {
          "type": "git",
          "url": "https://github.com/tuyoogame/YooAsset.git"
      }
  }
  ```
- 

## 项目定制教程

- 请去下载 [UniTask](https://github.com/Cysharp/UniTask) 源码
  - 注意不要用 `Sample` 里面的 `UniTask` 这个是专门给新手定制的
- 将 `Samples/UniTask Sample/UniTask/Runtime/External/YooAsset` 文件夹拷贝到 `UniTask/Runtime/External/YooAsset` 中
- 创建 `UniTask.YooAsset.asmdef` 文件
- 添加 `UniTask` 和 `YooAsset` 的引用
- 在 UniTask `_InternalVisibleTo.cs` 文件中增加 `[assembly: InternalsVisibleTo("UniTask.YooAsset")]` 后即可使用

## 有效性检查

一般使用项目定制时, 会出现如下警告, 这说明项目没有配置正确, 建议使用 **初学者定制的** 版本

```
yield BundledSceneProvider is not supported on await IEnumerator or Enumerator. ToUniTaskO, please use ToUniTask MonoBehaviou
coroutine Runner) instead
```

- 在 IDE 中点击 ToUniTask 跳转代码, 看是否可以正确跳转到 `UniTask/Runtime/External/YooAsset` 文件夹中
- 增加 `handle.ToUniTask(progress, timing)` 参数, 看是否有编译错误

如果不正确, 需要检查业务逻辑的 `asmdef` 是否引用正确, 假设你项目业务逻辑的 `asmdef` 名为 `View.asmdef`, 那么在 `View` 中, 要包含如下引用

- YooAsset
- UniTask
- UniTask.YooAsset

如果引用正确, 依然还有报错, 说明定制流程有问题, 请检查定制内容是否正确, 或者使用 **初学者定制的** 版本
