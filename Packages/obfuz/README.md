# Obfuz

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity Version](https://img.shields.io/badge/Unity-2019%2B-blue)](https://unity.com/)

Obfuz 是一款开源、强大、易用及稳定可靠的充分满足商业化游戏项目需求的Unity代码混淆和加固解决方案。

[English](./README-EN.md) | [中文](./README.md)

[Github](https://github.com/focus-creative-games/obfuz) | [Gitee](https://gitee.com/focus-creative-games/obfuz)

---

## 为什么选择 Obfuz？

- **开源免费**：基于 MIT 协议，免费使用和修改。
- **功能强大**：提供媲美商业工具的强大混淆和代码加固功能。
- **专为 Unity 设计**：为Unity工作流深度优化，自动化处理除了反射以外（因为工具做不到智能识别反射）所有需要特殊处理的情况（如MonoBehaviour名不能混淆），几乎零配置即可集成代码混淆功能。
- **稳定可靠**：有全面的自动化测试项目，成功通过3000个多个测试用例，几乎覆盖所有常见的代码用例
- **支持热更新**：支持HybridCLR、xlua之类最流行的代码热更新方案
- **敏捷开发**：快速响应开发者需求、迅速修复bug，及时跟进Unity及团结引擎的最新改动

## 功能特性

- **符号混淆**：支持丰富的配置规则和增量混淆，灵活高效地保护代码。
- **常量混淆**：混淆 `int`、`long`、`float`、`double`、`string`、数组 等常量，防止逆向工程。
- **变量内存加密**：加密内存中的变量，提升运行时安全。
- **函数调用混淆**：打乱函数调用结构，增加破解难度。
- **随机加密虚拟机**：生成随机化虚拟机，有效抵御反编译和破解工具。
- **静态与动态解密**：结合静态和动态解密，防止离线静态分析。
- **深度 Unity 集成**：与 Unity 工作流无缝衔接，简单配置即可使用。
- **热更新支持**：全面兼容 HybridCLR、xLua 等热更新框架，确保动态代码更新顺畅。
- **兼容DOTS**：兼容DOTS各个版本，无需配置即可正常工作。

## 支持的Unity版本与平台

- 支持Unity 2019+
- 支持团结引擎
- 支持Unity和团结引擎支持的所有平台
- 支持il2cpp和mono backend

## 文档

- [文档](https://www.obfuz.com/)
- [快速上手](https://www.obfuz.com/docs/beginner/quick-start)
- [示例项目](https://github.com/focus-creative-games/obfuz-samples)

## 未来计划

Obfuz 正在持续开发中，即将推出的功能包括：

- **表达式混淆**：混淆复杂表达式，进一步增强保护。
- **控制流混淆**：打乱代码执行流程，增加逆向难度。
- **代码水印**：嵌入可追踪的水印。
- **反内存转储与反调试**：防止内存转储和调试行为。
- **DLL 文件结构加密**：保护 DLL 文件结构免受篡改。
- **代码虚拟化**：将代码转化为虚拟化指令，提供最高级别安全。

## 许可证

Obfuz 采用 MIT 许可证发布，欢迎自由使用、修改和分发。

## 联系我们

如有问题、建议或错误报告，请在用以下方式联系我们：

- GitHub 上提交 Issue
- 邮件联系维护者：`obfuz@code-philosophy.com`
- 加入 **Luban&Obfuz交流群**，QQ群号： 692890842
