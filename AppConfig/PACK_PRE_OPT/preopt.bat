@title ´ò°üBatÔ¤´¦ÀíÖ´ÐÐ
@echo off

@set SRC_DIR=%CD%\..\..\
@echo %SRC_DIR%

@echo É¾³ý AotRes
rd /S /Q %SRC_DIR%\Assets\AotRes

@echo É¾³ý AotRes
rd /S /Q %SRC_DIR%\Assets\Plugins\Android\res~

@echo É¾³ýAnyThinkAds
rd /S /Q %SRC_DIR%\Assets\AnyThinkAds
del /Q %SRC_DIR%\Assets\AnyThinkAds.meta
rd /S /Q %SRC_DIR%\Assets\AnyThinkPlugin
del /Q %SRC_DIR%\Assets\AnyThinkPlugin.meta

@echo É¾³ý WeixinMinigame
rd /S /Q %SRC_DIR%\Packages\com.qq.weixin.minigame
@echo É¾³ý WX-WASM-SDK-V2
rd /S /Q %SRC_DIR%\Assets\WX-WASM-SDK-V2
del /Q %SRC_DIR%\Assets\WX-WASM-SDK-V2.meta
@echo É¾³ý WebGLTemplates
rd /S /Q %SRC_DIR%\Assets\WebGLTemplates
del /Q %SRC_DIR%\Assets\WebGLTemplates.meta


@echo É¾³ý ByteGame
rd /S /Q %SRC_DIR%\Assets\Plugins\ByteGame
del /Q %SRC_DIR%\Assets\Plugins\ByteGame.meta

@echo É¾³ý Tiktok
rd /S /Q %SRC_DIR%\Assets\Plugins\com.tiktok.minigame
del /Q %SRC_DIR%\Assets\Plugins\com.tiktok.minigame.meta

@echo É¾³ý Tiktok Desktop icon
rd /S /Q %SRC_DIR%\Assets\Res\TikTok
del /Q %SRC_DIR%\Assets\Res\TikTok.meta

::@echo É¾³ý cursor
::rd /S /Q %SRC_DIR%\Packages\com.boxqkrtm.ide.cursor

::@echo É¾³ý unity-mcp
::rd /S /Q %SRC_DIR%\Packages\com.coplaydev.unity-mcp

@echo Ô¤´¦ÀíÍê³É!
@echo on