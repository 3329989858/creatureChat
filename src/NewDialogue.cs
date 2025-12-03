/*using HUD;
using Menu.Remix.MixedUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace CreatureChat
{
    public class neweDialogBox : HudPart
    {
        // ==================== 字符控制相关类 ====================
        public class CharacterInfo
        {
            public char character;
            public string displayText; // 显示的实际字符串（可能是组合字符）
            public Color color;
            public float scale;
            public float offsetX;
            public float offsetY;
            public bool visible;
            public float rotation;
            public float alpha;
            public int animationTimer;
            public float shakeIntensity;
            public float waveAmplitude;
            public float waveSpeed;
            public float wavePhase;
            public float actualWidth; // 实际渲染宽度

            public CharacterInfo(string text)
            {
                displayText = text;
                character = text.Length > 0 ? text[0] : ' ';
                color = Color.white;
                scale = 1f;
                offsetX = 0f;
                offsetY = 0f;
                visible = true;
                rotation = 0f;
                alpha = 1f;
                animationTimer = 0;
                shakeIntensity = 0f;
                waveAmplitude = 0f;
                waveSpeed = 0f;
                wavePhase = 0f;
                actualWidth = 0f;
            }

            public void Update()
            {
                animationTimer++;

                // 震动效果
                if (shakeIntensity > 0f)
                {
                    offsetX = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                    offsetY = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                }

                // 波浪效果
                if (waveAmplitude > 0f)
                {
                    float wave = waveAmplitude * Mathf.Sin(wavePhase + animationTimer * waveSpeed * 0.1f);
                    offsetY = wave;
                }
            }
        }

        // ==================== 格式标记解析器 ====================
        public class FormatParser
        {
            public struct FormatTag
            {
                public int position;
                public string tag;
                public string value;
                public bool isStartTag;
            }

            public static string RemoveFormatTags(string text, out List<FormatTag> formatTags)
            {
                formatTags = new List<FormatTag>();
                if (string.IsNullOrEmpty(text)) return text;

                StringBuilder result = new StringBuilder();
                int position = 0;

                // 正则表达式匹配格式标记
                Regex regex = new Regex(@"<(\w+)(#?[\w\.]+)?>|</(\w+)end>");
                MatchCollection matches = regex.Matches(text);

                int lastIndex = 0;
                foreach (Match match in matches)
                {
                    // 添加匹配前的文本
                    result.Append(text.Substring(lastIndex, match.Index - lastIndex));

                    // 解析标记
                    if (match.Groups[3].Success) // 结束标记 </xxxend>
                    {
                        string tagName = match.Groups[3].Value;
                        formatTags.Add(new FormatTag
                        {
                            position = result.Length,
                            tag = tagName,
                            value = "",
                            isStartTag = false
                        });
                    }
                    else // 开始标记 <xxx value>
                    {
                        string tagName = match.Groups[1].Value;
                        string value = match.Groups[2].Success ? match.Groups[2].Value : "";
                        formatTags.Add(new FormatTag
                        {
                            position = result.Length,
                            tag = tagName,
                            value = value,
                            isStartTag = true
                        });
                    }

                    lastIndex = match.Index + match.Length;
                }

                // 添加最后一段文本
                result.Append(text.Substring(lastIndex));

                return result.ToString();
            }

            public static void ApplyFormatTags(List<CharacterInfo> characters, List<FormatTag> formatTags)
            {
                if (characters == null || formatTags == null || characters.Count == 0) return;

                // 按位置排序标记
                formatTags.Sort((a, b) => a.position.CompareTo(b.position));

                // 当前生效的格式栈
                Stack<FormatTag> activeFormats = new Stack<FormatTag>();

                int tagIndex = 0;
                for (int charIndex = 0; charIndex < characters.Count; charIndex++)
                {
                    // 应用当前位置的格式标记
                    while (tagIndex < formatTags.Count && formatTags[tagIndex].position == charIndex)
                    {
                        var tag = formatTags[tagIndex];

                        if (tag.isStartTag)
                        {
                            activeFormats.Push(tag);
                        }
                        else
                        {
                            // 弹出对应的开始标记
                            Stack<FormatTag> temp = new Stack<FormatTag>();
                            while (activeFormats.Count > 0 && activeFormats.Peek().tag != tag.tag)
                            {
                                temp.Push(activeFormats.Pop());
                            }
                            if (activeFormats.Count > 0) activeFormats.Pop();
                            // 恢复其他标记
                            while (temp.Count > 0) activeFormats.Push(temp.Pop());
                        }

                        tagIndex++;
                    }

                    // 应用所有活跃格式到当前字符
                    ApplyCurrentFormats(characters[charIndex], activeFormats);
                }
            }

            private static void ApplyCurrentFormats(CharacterInfo charInfo, Stack<FormatTag> activeFormats)
            {
                // 重置效果
                charInfo.color = Color.white;
                charInfo.scale = 1f;
                charInfo.shakeIntensity = 0f;
                charInfo.waveAmplitude = 0f;
                charInfo.rotation = 0f;
                charInfo.alpha = 1f;

                // 反向遍历栈（从最早到最晚）
                var formatsArray = activeFormats.ToArray();
                for (int i = formatsArray.Length - 1; i >= 0; i--)
                {
                    var tag = formatsArray[i];

                    switch (tag.tag.ToLower())
                    {
                        case "color":
                            if (!string.IsNullOrEmpty(tag.value))
                            {
                                charInfo.color = ParseColor(tag.value);
                            }
                            break;

                        case "scale":
                            if (float.TryParse(tag.value, out float scale))
                            {
                                charInfo.scale = scale;
                            }
                            break;

                        case "shake":
                            if (float.TryParse(tag.value, out float shake))
                            {
                                charInfo.shakeIntensity = shake;
                            }
                            break;

                        case "wave":
                            if (!string.IsNullOrEmpty(tag.value))
                            {
                                var parts = tag.value.Split(',');
                                if (parts.Length >= 1 && float.TryParse(parts[0], out float amp))
                                    charInfo.waveAmplitude = amp;
                                if (parts.Length >= 2 && float.TryParse(parts[1], out float speed))
                                    charInfo.waveSpeed = speed;
                                if (parts.Length >= 3 && float.TryParse(parts[2], out float phase))
                                    charInfo.wavePhase = phase;
                            }
                            break;

                        case "rotate":
                            if (float.TryParse(tag.value, out float rotate))
                            {
                                charInfo.rotation = rotate;
                            }
                            break;

                        case "alpha":
                            if (float.TryParse(tag.value, out float alpha))
                            {
                                charInfo.alpha = Mathf.Clamp01(alpha);
                            }
                            break;

                        case "rainbow":
                            // 彩虹效果：根据字符位置和动画时间计算颜色
                            float hue = (charInfo.animationTimer * 0.01f) % 1f;
                            charInfo.color = Color.HSVToRGB(hue, 1f, 1f);
                            break;
                    }
                }
            }

            private static Color ParseColor(string colorStr)
            {
                if (colorStr.StartsWith("#"))
                {
                    // 十六进制颜色 #RRGGBB 或 #RRGGBBAA
                    colorStr = colorStr.Substring(1);
                    if (colorStr.Length == 6)
                    {
                        // RRGGBB
                        byte r = Convert.ToByte(colorStr.Substring(0, 2), 16);
                        byte g = Convert.ToByte(colorStr.Substring(2, 2), 16);
                        byte b = Convert.ToByte(colorStr.Substring(4, 2), 16);
                        return new Color(r / 255f, g / 255f, b / 255f);
                    }
                    else if (colorStr.Length == 8)
                    {
                        // RRGGBBAA
                        byte r = Convert.ToByte(colorStr.Substring(0, 2), 16);
                        byte g = Convert.ToByte(colorStr.Substring(2, 2), 16);
                        byte b = Convert.ToByte(colorStr.Substring(4, 2), 16);
                        byte a = Convert.ToByte(colorStr.Substring(6, 2), 16);
                        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                    }
                }
                else if (colorStr.Contains("."))
                {
                    // RGBA值 color.1.0.0.1
                    var parts = colorStr.Split('.');
                    if (parts.Length == 4)
                    {
                        float r = float.Parse(parts[0]);
                        float g = float.Parse(parts[1]);
                        float b = float.Parse(parts[2]);
                        float a = float.Parse(parts[3]);
                        return new Color(r, g, b, a);
                    }
                }
                else
                {
                    // 预定义颜色名
                    switch (colorStr.ToLower())
                    {
                        case "red": return Color.red;
                        case "green": return Color.green;
                        case "blue": return Color.blue;
                        case "yellow": return Color.yellow;
                        case "cyan": return Color.cyan;
                        case "magenta": return Color.magenta;
                        case "white": return Color.white;
                        case "black": return Color.black;
                        case "gray": return Color.gray;
                        case "orange": return new Color(1f, 0.5f, 0f);
                        case "purple": return new Color(0.5f, 0f, 1f);
                    }
                }

                return Color.white;
            }

            // 分割文本为字素簇（处理组合字符）
            public static List<string> SplitIntoGraphemeClusters(string text)
            {
                var clusters = new List<string>();
                var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(text);

                while (enumerator.MoveNext())
                {
                    clusters.Add(enumerator.GetTextElement());
                }

                return clusters;
            }
        }

        public class Message
        {
            public float xOrientation;
            public float yPos;
            public string text;
            public string processedText; // 处理后的文本（移除格式标记）
            public int linger;
            public int lines;
            public int longestLine;
            public List<FormatParser.FormatTag> formatTags; // 格式标记信息
            public List<string> graphemeClusters; // 字素簇列表

            public Message(string text, float xOrientation, float yPos, int extraLinger)
            {
                this.text = Custom.ReplaceLineDelimeters(text);
                this.xOrientation = xOrientation;

                // 解析格式标记
                this.processedText = FormatParser.RemoveFormatTags(this.text, out formatTags);

                // 分割为字素簇
                graphemeClusters = FormatParser.SplitIntoGraphemeClusters(processedText);

                // 基于处理后的文本计算
                string[] array = Regex.Split(processedText.Replace("<WWLINE>", ""), "<LINE>");
                for (int i = 0; i < array.Length; i++)
                {
                    longestLine = Math.Max(longestLine, array[i].Length);
                }

                lines = array.Length;
                this.yPos = yPos + (20f + 15f * (float)lines);
                linger = (int)Mathf.Lerp((float)processedText.Length * 2f, 80f, 0.5f) + extraLinger;
            }
        }

        // ==================== 公共字段 ====================
        public PhysicalObject chatter;
        public float defaultXOrientation = 0.5f;
        public float defaultYPos;
        public FLabel label; // 用于测量文本宽度
        public FSprite[] sprites;
        public List<Message> messages;
        public int showCharacter;
        public string showText;
        public float sizeFac;
        public float lastSizeFac;
        public float width;
        public int lingerCounter;
        public bool permanentDisplay;
        public float actualWidth;
        public Color currentColor;
        public int showDelay;

        // 字符控制相关字段
        public List<List<CharacterInfo>> characterInfos = new List<List<CharacterInfo>>();
        private List<List<FLabel>> characterLabels = new List<List<FLabel>>();
        private List<List<float>> characterWidths = new List<List<float>>(); // 存储每个字符的实际宽度
        private bool useCharacterMode = true;

        // 静态常量
        public static float lineHeight = 15f;
        public static float heightMargin = 20f;
        public static float widthMargin = 30f;

        public int MainFillSprite => 8;

        public Message CurrentMessage
        {
            get
            {
                if (messages.Count < 1)
                {
                    return null;
                }

                return messages[0];
            }
        }

        public bool ShowingAMessage => CurrentMessage != null;

        // ==================== 精灵索引方法 ====================
        public int SideSprite(int side)
        {
            return 9 + side;
        }

        public int CornerSprite(int corner)
        {
            return 13 + corner;
        }

        public int FillSideSprite(int side)
        {
            return side;
        }

        public int FillCornerSprite(int corner)
        {
            return 4 + corner;
        }

        // ==================== 核心方法 ====================
        public Vector2 DrawPos(float timeStacker)
        {
            if (chatter == null)
            {
                if (CurrentMessage == null) return Vector2.zero;
                return new Vector2(CurrentMessage.xOrientation * hud.rainWorld.screenSize.x, CurrentMessage.yPos + hud.rainWorld.options.SafeScreenOffset.y);
            }
            else
            {
                if (chatter.room != null)
                {
                    Vector2 CreaturePos = (Vector2.Lerp(chatter.firstChunk.lastPos, chatter.firstChunk.pos, timeStacker) - chatter.room.game.cameras[0].pos);
                    CreaturePos.y += 50;
                    return CreaturePos;
                }
            }
            return Vector2.zero;
        }

        public static int GetDelay()
        {
            if (Custom.rainWorld.options.language == InGameTranslator.LanguageID.Japanese)
            {
                return 2;
            }

            if (Custom.rainWorld.options.language == InGameTranslator.LanguageID.Korean)
            {
                return 2;
            }

            if (Custom.rainWorld.options.language == InGameTranslator.LanguageID.Chinese)
            {
                return 3;
            }

            return 1;
        }

        // ==================== 构造函数 ====================
        public CreatureDialogBox(HUD.HUD hud, PhysicalObject chatter = null, bool useCharacterMode = true)
            : base(hud)
        {
            messages = new List<Message>();
            currentColor = Color.white;
            this.chatter = chatter;
            this.useCharacterMode = useCharacterMode;
            InitiateSprites();
        }

        // ==================== 字符标签管理 ====================
        private void ClearCharacterLabels()
        {
            foreach (var line in characterLabels)
            {
                foreach (var label in line)
                {
                    label.RemoveFromContainer();
                }
            }
            characterLabels.Clear();
            characterInfos.Clear();
            characterWidths.Clear();
        }

        private void CreateCharacterLabelsForMessage(Message message)
        {
            ClearCharacterLabels();

            if (message == null || string.IsNullOrEmpty(message.processedText) || !useCharacterMode) return;

            // 分割文本为行
            string[] lines = Regex.Split(message.processedText.Replace("<WWLINE>", ""), "<LINE>");

            // 临时标签用于测量宽度
            FLabel measureLabel = new FLabel(Custom.GetFont(), "");

            foreach (var line in lines)
            {
                // 分割行为字素簇
                var clusters = FormatParser.SplitIntoGraphemeClusters(line);

                var lineChars = new List<CharacterInfo>();
                var lineLabels = new List<FLabel>();
                var lineWidths = new List<float>();

                foreach (var cluster in clusters)
                {
                    var charInfo = new CharacterInfo(cluster);
                    lineChars.Add(charInfo);

                    // 创建显示标签
                    FLabel charLabel = new FLabel(Custom.GetFont(), cluster);
                    charLabel.alignment = FLabelAlignment.Left;
                    charLabel.color = Color.white;
                    charLabel.scale = 1f;
                    hud.fContainers[1].AddChild(charLabel);
                    lineLabels.Add(charLabel);

                    // 测量实际宽度
                    measureLabel.text = cluster;
                    float clusterWidth = measureLabel.textRect.width;
                    charInfo.actualWidth = clusterWidth;
                    lineWidths.Add(clusterWidth);
                }

                characterInfos.Add(lineChars);
                characterLabels.Add(lineLabels);
                characterWidths.Add(lineWidths);
            }

            measureLabel.RemoveFromContainer();

            // 应用格式标记
            FormatParser.ApplyFormatTags(FlattenCharacterList(), message.formatTags);

            // 更新所有字符标签
            UpdateAllCharacterLabels();

            // 重新计算总宽度
            RecalculateActualWidth();
        }

        private void RecalculateActualWidth()
        {
            if (characterWidths.Count == 0)
            {
                actualWidth = 0f;
                return;
            }

            float maxLineWidth = 0f;
            for (int lineIdx = 0; lineIdx < characterWidths.Count; lineIdx++)
            {
                float lineWidth = 0f;
                foreach (var width in characterWidths[lineIdx])
                {
                    lineWidth += width;
                }
                maxLineWidth = Mathf.Max(maxLineWidth, lineWidth);
            }

            actualWidth = maxLineWidth;
        }

        private List<CharacterInfo> FlattenCharacterList()
        {
            var result = new List<CharacterInfo>();
            foreach (var line in characterInfos)
            {
                result.AddRange(line);
            }
            return result;
        }

        private void UpdateCharacterLabel(int lineIndex, int charIndex)
        {
            if (!useCharacterMode || lineIndex >= characterLabels.Count || charIndex >= characterLabels[lineIndex].Count)
                return;

            var charInfo = characterInfos[lineIndex][charIndex];
            var label = characterLabels[lineIndex][charIndex];

            label.color = new Color(charInfo.color.r, charInfo.color.g, charInfo.color.b, charInfo.alpha);
            label.scale = charInfo.scale;
            label.rotation = charInfo.rotation;
            label.isVisible = charInfo.visible;
        }

        private void UpdateAllCharacterLabels()
        {
            if (!useCharacterMode) return;

            for (int lineIdx = 0; lineIdx < characterLabels.Count; lineIdx++)
            {
                for (int charIdx = 0; charIdx < characterLabels[lineIdx].Count; charIdx++)
                {
                    UpdateCharacterLabel(lineIdx, charIdx);
                }
            }
        }

        // ==================== 消息管理方法 ====================
        public override void Update()
        {
            if (CurrentMessage == null)
            {
                return;
            }

            lastSizeFac = sizeFac;
            if (sizeFac < 1f && lingerCounter < 1)
            {
                sizeFac = Mathf.Min(sizeFac + 1f / 6f, 1f);
            }
            else
            {
                if (hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player && (hud.owner as Player).abstractCreature.world.game.pauseMenu != null)
                {
                    return;
                }

                if (permanentDisplay)
                {
                    showDelay = 0;
                    showCharacter = CurrentMessage.processedText.Length;
                    showText = CurrentMessage.processedText.Substring(0, showCharacter);
                }
                else if (showCharacter < CurrentMessage.processedText.Length)
                {
                    showDelay++;
                    if (showDelay >= GetDelay())
                    {
                        showDelay = 0;
                        showCharacter++;
                        showText = CurrentMessage.processedText.Substring(0, showCharacter);

                        // 更新字符显示状态
                        UpdateCharacterVisibility();
                    }
                }
                else
                {
                    if (hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player || (hud.owner as Player).abstractCreature.world.game.pauseMenu == null)
                    {
                        lingerCounter++;
                    }

                    if (lingerCounter > CurrentMessage.linger)
                    {
                        showText = "";
                        if (sizeFac > 0f)
                        {
                            sizeFac = Mathf.Max(0f, sizeFac - 1f / 6f);
                        }
                        else
                        {
                            messages.RemoveAt(0);
                            if (messages.Count > 0)
                            {
                                InitNextMessage();
                            }
                            else
                            {
                                ClearCharacterLabels();
                            }
                        }
                    }
                }

                // 更新字符动画
                if (useCharacterMode)
                {
                    for (int lineIdx = 0; lineIdx < characterInfos.Count; lineIdx++)
                    {
                        for (int charIdx = 0; charIdx < characterInfos[lineIdx].Count; charIdx++)
                        {
                            characterInfos[lineIdx][charIdx].Update();
                        }
                    }
                    UpdateAllCharacterLabels();
                }

                if (ShowingAMessage && hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player && (hud.owner as Player).graphicsModule != null && (hud.owner as Player).abstractCreature.world.game.IsStorySession && (hud.owner as Player).abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
                {
                    ((hud.owner as Player).graphicsModule as PlayerGraphics).markBaseAlpha = Mathf.Min(1f, ((hud.owner as Player).graphicsModule as PlayerGraphics).markBaseAlpha + 0.005f);
                    ((hud.owner as Player).graphicsModule as PlayerGraphics).markAlpha = Mathf.Min(0.5f + UnityEngine.Random.value, 1f);
                }
            }
        }

        private void UpdateCharacterVisibility()
        {
            if (!useCharacterMode) return;

            int charsShown = 0;
            for (int lineIdx = 0; lineIdx < characterLabels.Count; lineIdx++)
            {
                for (int charIdx = 0; charIdx < characterLabels[lineIdx].Count; charIdx++)
                {
                    bool shouldShow = charsShown < showCharacter;
                    if (characterInfos[lineIdx][charIdx].visible != shouldShow)
                    {
                        characterInfos[lineIdx][charIdx].visible = shouldShow;
                        characterLabels[lineIdx][charIdx].isVisible = shouldShow;
                    }
                    charsShown++;
                }
                // 跳过换行符
                if (lineIdx < characterLabels.Count - 1)
                {
                    charsShown++;
                }
            }
        }

        public void Interrupt(string text, int extraLinger)
        {
            if (messages.Count > 0)
            {
                messages = new List<Message> { messages[0] };
                lingerCounter = messages[0].linger + 1;
                showCharacter = messages[0].processedText.Length + 2;
            }

            NewMessage(text, defaultXOrientation, defaultYPos, extraLinger);
        }

        public void NewMessage(string text, int extraLinger)
        {
            NewMessage(text, defaultXOrientation, defaultYPos, extraLinger);
        }

        public void NewMessage(string text, float xOrientation, float yPos, int extraLinger)
        {
            messages.Add(new Message(text, xOrientation, yPos, extraLinger));
            if (messages.Count == 1)
            {
                InitNextMessage();
            }
        }

        public void Interrupt(Message message)
        {
            if (messages.Count > 0)
            {
                messages = new List<Message> { messages[0] };
                lingerCounter = messages[0].linger + 1;
                showCharacter = messages[0].processedText.Length + 2;
            }

            NewMessage(message);
        }

        public void NewMessage(Message message)
        {
            messages.Add(message);
            if (messages.Count == 1)
            {
                InitNextMessage();
            }
        }

        public void InitNextMessage()
        {
            showCharacter = 0;
            showText = "";
            lastSizeFac = 0f;
            sizeFac = 0f;
            lingerCounter = 0;

            // 设置label文本用于宽度测量
            label.text = CurrentMessage.processedText;
            actualWidth = label.textRect.width;
            label.text = "";

            // 为字符模式创建标签
            if (useCharacterMode && CurrentMessage != null)
            {
                CreateCharacterLabelsForMessage(CurrentMessage);
            }
        }

        // ==================== 精灵初始化 ====================
        public void InitiateSprites()
        {
            sprites = new FSprite[17];
            for (int i = 0; i < 4; i++)
            {
                sprites[SideSprite(i)] = new FSprite("pixel");
                sprites[SideSprite(i)].scaleY = 2f;
                sprites[SideSprite(i)].scaleX = 2f;
                sprites[CornerSprite(i)] = new FSprite("UIroundedCorner");
                sprites[FillSideSprite(i)] = new FSprite("pixel");
                sprites[FillSideSprite(i)].scaleY = 6f;
                sprites[FillSideSprite(i)].scaleX = 6f;
                sprites[FillCornerSprite(i)] = new FSprite("UIroundedCornerInside");
            }

            sprites[SideSprite(0)].anchorY = 0f;
            sprites[SideSprite(2)].anchorY = 0f;
            sprites[SideSprite(1)].anchorX = 0f;
            sprites[SideSprite(3)].anchorX = 0f;
            sprites[CornerSprite(0)].scaleY = -1f;
            sprites[CornerSprite(2)].scaleX = -1f;
            sprites[CornerSprite(3)].scaleY = -1f;
            sprites[CornerSprite(3)].scaleX = -1f;
            sprites[MainFillSprite] = new FSprite("pixel");
            sprites[MainFillSprite].anchorY = 0f;
            sprites[MainFillSprite].anchorX = 0f;
            sprites[FillSideSprite(0)].anchorY = 0f;
            sprites[FillSideSprite(2)].anchorY = 0f;
            sprites[FillSideSprite(1)].anchorX = 0f;
            sprites[FillSideSprite(3)].anchorX = 0f;
            sprites[FillCornerSprite(0)].scaleY = -1f;
            sprites[FillCornerSprite(2)].scaleX = -1f;
            sprites[FillCornerSprite(3)].scaleY = -1f;
            sprites[FillCornerSprite(3)].scaleX = -1f;
            for (int j = 0; j < 9; j++)
            {
                sprites[j].color = new Color(0f, 0f, 0f);
                sprites[j].alpha = 0.75f;
            }

            label = new FLabel(Custom.GetFont(), "");
            label.alignment = FLabelAlignment.Left;
            label.anchorX = 0f;
            label.anchorY = 1f;
            for (int k = 0; k < sprites.Length; k++)
            {
                hud.fContainers[1].AddChild(sprites[k]);
            }

            hud.fContainers[1].AddChild(label);
        }

        // ==================== 绘制方法 ====================
        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].isVisible = CurrentMessage != null;
            }
            label.isVisible = CurrentMessage != null && !useCharacterMode;

            if (chatter != null && chatter.room == null)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    sprites[i].isVisible = false;
                }
                label.isVisible = false;
            }

            if (CurrentMessage != null)
            {
                float num = lineHeight;
                Vector2 vector = DrawPos(timeStacker);
                Vector2 vector2 = new Vector2(0f, heightMargin + num * (float)CurrentMessage.lines);
                if (Custom.GetFont().Contains("Full"))
                {
                    vector2.y += LabelTest.LineHalfHeight(bigText: false);
                }

                vector2.x = widthMargin + actualWidth;
                vector2.x = Mathf.Lerp(40f, vector2.x, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastSizeFac, sizeFac, timeStacker)), 0.5f));
                vector2.y *= 0.5f + 0.5f * Mathf.Lerp(lastSizeFac, sizeFac, timeStacker);
                vector.x -= 1f / 3f;
                vector.y -= 1f / 3f;

                // 绘制背景精灵
                DrawBackgroundSprites(vector, vector2);

                // 绘制文本
                if (useCharacterMode)
                {
                    DrawCharacterModeText(vector, vector2, num, timeStacker);
                }
                else
                {
                    DrawNormalText(vector, vector2, num);
                }
            }
        }

        private void DrawBackgroundSprites(Vector2 vector, Vector2 vector2)
        {
            label.color = currentColor;
            label.x = vector.x - actualWidth * 0.5f;
            label.y = vector.y + vector2.y / 2f - lineHeight * 0.6666f;
            label.text = useCharacterMode ? "" : showText;

            vector.x -= vector2.x / 2f;
            vector.y -= vector2.y / 2f;

            sprites[SideSprite(0)].x = vector.x + 1f;
            sprites[SideSprite(0)].y = vector.y + 6f;
            sprites[SideSprite(0)].scaleY = vector2.y - 12f;
            sprites[SideSprite(1)].x = vector.x + 6f;
            sprites[SideSprite(1)].y = vector.y + vector2.y - 1f;
            sprites[SideSprite(1)].scaleX = vector2.x - 12f;
            sprites[SideSprite(2)].x = vector.x + vector2.x - 1f;
            sprites[SideSprite(2)].y = vector.y + 6f;
            sprites[SideSprite(2)].scaleY = vector2.y - 12f;
            sprites[SideSprite(3)].x = vector.x + 6f;
            sprites[SideSprite(3)].y = vector.y + 1f;
            sprites[SideSprite(3)].scaleX = vector2.x - 12f;

            sprites[CornerSprite(0)].x = vector.x + 3.5f;
            sprites[CornerSprite(0)].y = vector.y + 3.5f;
            sprites[CornerSprite(1)].x = vector.x + 3.5f;
            sprites[CornerSprite(1)].y = vector.y + vector2.y - 3.5f;
            sprites[CornerSprite(2)].x = vector.x + vector2.x - 3.5f;
            sprites[CornerSprite(2)].y = vector.y + vector2.y - 3.5f;
            sprites[CornerSprite(3)].x = vector.x + vector2.x - 3.5f;
            sprites[CornerSprite(3)].y = vector.y + 3.5f;

            Color color = new Color(1f, 1f, 1f);
            for (int j = 0; j < 4; j++)
            {
                sprites[SideSprite(j)].color = color;
                sprites[CornerSprite(j)].color = color;
            }

            sprites[FillSideSprite(0)].x = vector.x + 4f;
            sprites[FillSideSprite(0)].y = vector.y + 7f;
            sprites[FillSideSprite(0)].scaleY = vector2.y - 14f;
            sprites[FillSideSprite(1)].x = vector.x + 7f;
            sprites[FillSideSprite(1)].y = vector.y + vector2.y - 4f;
            sprites[FillSideSprite(1)].scaleX = vector2.x - 14f;
            sprites[FillSideSprite(2)].x = vector.x + vector2.x - 4f;
            sprites[FillSideSprite(2)].y = vector.y + 7f;
            sprites[FillSideSprite(2)].scaleY = vector2.y - 14f;
            sprites[FillSideSprite(3)].x = vector.x + 7f;
            sprites[FillSideSprite(3)].y = vector.y + 4f;
            sprites[FillSideSprite(3)].scaleX = vector2.x - 14f;

            sprites[FillCornerSprite(0)].x = vector.x + 3.5f;
            sprites[FillCornerSprite(0)].y = vector.y + 3.5f;
            sprites[FillCornerSprite(1)].x = vector.x + 3.5f;
            sprites[FillCornerSprite(1)].y = vector.y + vector2.y - 3.5f;
            sprites[FillCornerSprite(2)].x = vector.x + vector2.x - 3.5f;
            sprites[FillCornerSprite(2)].y = vector.y + vector2.y - 3.5f;
            sprites[FillCornerSprite(3)].x = vector.x + vector2.x - 3.5f;
            sprites[FillCornerSprite(3)].y = vector.y + 3.5f;

            sprites[MainFillSprite].x = vector.x + 7f;
            sprites[MainFillSprite].y = vector.y + 7f;
            sprites[MainFillSprite].scaleX = vector2.x - 14f;
            sprites[MainFillSprite].scaleY = vector2.y - 14f;
        }

        private void DrawCharacterModeText(Vector2 basePos, Vector2 bubbleSize, float lineHeight, float timeStacker)
        {
            if (characterLabels.Count == 0) return;

            float textStartY = basePos.y + bubbleSize.y / 2f - lineHeight * 0.6666f;

            // ==================== 添加字符间隔常量 ====================
            float characterSpacing = 1f; // 增加1像素的字符间隔

            for (int lineIdx = 0; lineIdx < characterLabels.Count; lineIdx++)
            {
                float lineY = textStartY - (lineIdx * lineHeight);

                // 计算当前行的总宽度（包括间隔）
                float lineTotalWidth = 0f;
                for (int i = 0; i < characterWidths[lineIdx].Count; i++)
                {
                    lineTotalWidth += characterWidths[lineIdx][i];
                    // 为除最后一个字符外的所有字符添加间隔
                    if (i < characterWidths[lineIdx].Count - 1)
                    {
                        lineTotalWidth += characterSpacing;
                    }
                }

                // 计算行起始X位置（居中）
                float lineStartX = basePos.x - bubbleSize.x / 2f + widthMargin / 2f;
                float currentX = lineStartX;

                for (int charIdx = 0; charIdx < characterLabels[lineIdx].Count; charIdx++)
                {
                    var label = characterLabels[lineIdx][charIdx];
                    var charInfo = characterInfos[lineIdx][charIdx];

                    // 获取字符的实际宽度
                    float charWidth = charInfo.actualWidth;

                    // 计算字符的水平中心位置
                    float charCenterX = currentX + charWidth * 0.5f;

                    // 应用格式效果偏移
                    float finalX = charCenterX + charInfo.offsetX;
                    float finalY = lineY + charInfo.offsetY;

                    // 应用旋转
                    label.x = finalX;
                    label.y = finalY;
                    label.scale = charInfo.scale;
                    label.rotation = charInfo.rotation;
                    label.color = new Color(charInfo.color.r, charInfo.color.g, charInfo.color.b, charInfo.alpha);

                    // 计算哪些字符应该显示
                    int charsBefore = 0;
                    for (int i = 0; i < lineIdx; i++)
                    {
                        charsBefore += characterLabels[i].Count;
                    }
                    int currentCharIndex = charsBefore + charIdx;

                    label.isVisible = charInfo.visible && currentCharIndex < showCharacter;

                    // 更新下一个字符的起始位置（加上间隔）
                    currentX += charWidth;
                    // 如果不是最后一个字符，添加间隔
                    if (charIdx < characterLabels[lineIdx].Count - 1)
                    {
                        currentX += characterSpacing;
                    }
                }
            }
        }

        private void DrawNormalText(Vector2 vector, Vector2 vector2, float lineHeight)
        {
            label.color = currentColor;
            label.x = vector.x - actualWidth * 0.5f;
            label.y = vector.y + vector2.y / 2f - lineHeight * 0.6666f;
            label.text = showText;
        }

        public void EndCurrentMessageNow()
        {
            if (messages.Count == 0) return;

            messages.RemoveAt(0);

            showText = "";
            showCharacter = 0;
            lingerCounter = 0;
            sizeFac = 0f;
            lastSizeFac = 0f;

            ClearCharacterLabels();

            if (messages.Count > 0)
                InitNextMessage();
            else
                label.text = "";
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            ClearCharacterLabels();
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].RemoveFromContainer();
            }
        }
    }
}*/