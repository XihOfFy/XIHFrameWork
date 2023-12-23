namespace FairyGUI
{
	public static class Extensions
	{
		private static char[] componentPathseparator = new char[] { '\\', '/' };
		public static Controller GetControllerFullPath(this GComponent component, string path)
		{
			if (component == null || path == null || path.Length <= 0)
				return null;

			var names = path.Split(componentPathseparator);
			if (names.Length == 1)
				return component.GetController(names[0]);
			else
			{
				var temp = component;
				for (int i = 0; i < names.Length && temp != null; i++)
				{
					if (i == names.Length - 1)
						return temp.GetController(names[i]);
					else
						temp = component.GetChildByPath(names[i])?.asCom;
				}

				return null;
			}
		}

		public static void SetUrl(this GLoader gLoader,string url)
		{
			if (gLoader != null)
				gLoader.url = url;
		}

		public static void SetText(this GTextField textField, string text)
		{
			if (textField != null)
				textField.text = text;
		}

		public static bool TrySetIndex(this Controller controller, int index, bool dispatchOnChanged = false)
		{
			if (controller == null)
				return false;

			if (index < 0 || index >= controller.pageCount)
				return false;

			if (dispatchOnChanged)
				controller.selectedIndex = index;
			else
				controller.SetSelectedIndex(index);

			return true;
		}

		public static void SetCutText(this GTextField textField, string text, string replaceStr)
		{
			if (textField == null)
				return;

			// 空文本
			if (string.IsNullOrEmpty(text))
			{
				textField.text = string.Empty;
				return;
			}

			// 非单行文本组件 
			if (!textField.singleLine)
			{
				textField.text = text;
				return;
			}

			// 自动宽高模式
			if (textField.autoSize != AutoSizeType.None)
			{
				textField.text = text;
				return;
			}

			BaseFont font;
			if (textField.textFormat.font != null)
				font = FontManager.GetFont(textField.textFormat.font);
			else
				font = FontManager.GetFont(UIConfig.defaultFont);

			if (font == null)
				return;

			// 默认字体大小缩放倍数为1
			font.SetFormat(textField.textFormat, 1);
			font.PrepareCharacters(text);

			float lineWidth = 0, replaceStrWidth = 0;

			// 计算替换部分长度
			for (int i = 0; i < replaceStr.Length; i++)
			{
				char ch = replaceStr[i];
				font.GetGlyph(ch, out var glyphWidth, out var glyphHeight, out var glyphBaseline);

				replaceStrWidth += glyphWidth;

				if (i < replaceStr.Length - 1)
					replaceStrWidth += textField.textFormat.letterSpacing;
			}

			int checkIndex = -1;
			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];
				font.GetGlyph(ch, out var glyphWidth, out var glyphHeight, out var glyphBaseline);

				// 字宽
				lineWidth += glyphWidth;

				if (i < text.Length - 1)
				{
					// 字距
					lineWidth += textField.textFormat.letterSpacing;

					//	截取到checkIndex位置的长度 + 添加尾部部分长度 可以完整显示
					if ((lineWidth + replaceStrWidth) < textField.width)
					{
						checkIndex = i;
					}
				}
				else
				{
					//	全部文本可以显示
					if (lineWidth < textField.width)
					{
						checkIndex = i;
					}
				}
			}

			// 处理结果
			if (checkIndex == text.Length - 1)
			{
				// 全显示
				textField.text = text;
			}
			else
			{
				// 部分显示
				textField.text = text.Substring(0, checkIndex + 1) + replaceStr;
			}
		}
	}
}
