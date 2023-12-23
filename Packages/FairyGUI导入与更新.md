## FairyGUI版本

[Releases · fairygui/FairyGUI-unity (github.com)](https://github.com/fairygui/FairyGUI-unity/releases)

## FairyGUI导入Unity或更新

- 将FairyGUI Git仓库中`Assets`的文件夹拷贝到项目的`Packages/`下

- 修改`Assets`名字为对应版本名字,例如:`FairyGUI-unity-4.3.0`

- 进入此文件夹，新建`package.json`文件，修改对应`"version`
  
  ```
  {
    "name": "unity.fairygui",
    "version": "4.3.0",
    "displayName": "FairyGUI",
    "description": "FairyGUI unity",
    "category": "Runtime",
    "keywords": [
      "FairyGUI"
    ],
    "samples": [
      {
        "displayName": "Examples",
        "description": "包含各种使用示例",
        "path": "Examples~"
      }
    ],
    "repository": {
      "url": "https://github.com/fairygui/FairyGUI-unity/releases",
      "type": "git"
    }
  }
  ```

- 找到`Examples`文件夹，将`Examples`文件名修改为`Examples~`

- 删除`StreamingAssets`文件夹和对应`meta`文件

- Unity打开项目，进入`Packages/unity.fairygui`文件夹，新建`FairyGUI.asmdef`文件
  
  ```
  {
      "name": "FairyGUI",
      "rootNamespace": "",
      "references": [],
      "includePlatforms": [],
      "excludePlatforms": [],
      "allowUnsafeCode": false,
      "overrideReferences": false,
      "precompiledReferences": [],
      "autoReferenced": true,
      "defineConstraints": [],
      "versionDefines": [],
      "noEngineReferences": false
  }
  ```

- Unity打开项目，进入`Packages/unity.fairygui/Scripts/Editor`文件夹，新建`FairyGUI.Editor.asmdef`文件
  
  ```
  {
      "name": "FairyGUI.Editor",
      "rootNamespace": "",
      "references": [
          "FairyGUI"
      ],
      "includePlatforms": [
          "Editor"
      ],
      "excludePlatforms": [],
      "allowUnsafeCode": false,
      "overrideReferences": false,
      "precompiledReferences": [],
      "autoReferenced": true,
      "defineConstraints": [],
      "versionDefines": [],
      "noEngineReferences": false
  }
  ```
