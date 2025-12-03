using HUD;
using Menu.Remix.MixedUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CreatureChat
{
    public class CreatureDialogBox : HudPart
    {
        public class Message
        {
            public float xOrientation;

            public float yPos;

            public string text;

            public int linger;

            public int lines;

            public int longestLine;

            public Message(string text, float xOrientation, float yPos, int extraLinger)
            {
                this.text = Custom.ReplaceLineDelimeters(text);
                this.xOrientation = xOrientation;
                linger = (int)Mathf.Lerp((float)text.Length * 2f, 80f, 0.5f) + extraLinger;
                string[] array = Regex.Split(text.Replace("<WWLINE>", ""), "<LINE>");
                for (int i = 0; i < array.Length; i++)
                {
                    longestLine = Math.Max(longestLine, array[i].Length);
                }

                lines = array.Length;
                this.yPos = yPos + (20f + 15f * (float)lines);
            }
        }

        public class CharactorEffect
        {
            public FLabel Owner;
            public FLabel FLabel;
            public Color Color;
            public bool Rainbow;
            public float Alpha;
            public float ShakeIntensity;
            public float WaveIntensity;
            public float WaveCounter;
            public float WaveSpeed;
            public float WaveDensity;

            public CharactorEffect(FLabel owner, FLabel fLabel)
            {
                Owner = owner;
                FLabel = fLabel;
                Color = owner.color;
                Alpha = owner.alpha;
                ShakeIntensity = 0;
                WaveIntensity = 0;
                WaveSpeed = 5;
                WaveDensity = 15;
                FLabel = fLabel;
            }

            public void update()
            {
                WaveCounter += Time.deltaTime*WaveSpeed;
                if (Owner != null)
                {
                    if (Rainbow)
                    {
                        Color.RGBToHSV(Color, out float H, out float S, out float V);
                        Color = Color.HSVToRGB(H + 0.02f, S, V);
                    }
                    Owner.color = Color;
                    Owner.alpha = Alpha;
                    Owner.y += Mathf.Sin((Owner.x - FLabel.x)/ WaveDensity + WaveCounter) * WaveIntensity;
                    Owner.x += Random.Range(-ShakeIntensity, ShakeIntensity);
                    Owner.y += Random.Range(-ShakeIntensity, ShakeIntensity);
                }
            }
        }

        public struct CharStyle
        {
            public Color color;
            public float shake;
            public float wave;
            public bool rainbow;
            public static CharStyle Default => new CharStyle
            {
                color = Color.white,
                rainbow = false
            };
        }

        public PhysicalObject chatter;

        public float defaultXOrientation = 0.5f;

        public float defaultYPos;


        public FLabel label;

        public List<FLabel> charactors = new List<FLabel>();

        public List<CharactorEffect> charactorEffects = new List<CharactorEffect>();

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

        public static float meanCharWidth = 6f;

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

        public Vector2 DrawPos(float timeStacker)
        {
            if (chatter == null)
            {
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

        public CreatureDialogBox(HUD.HUD hud, PhysicalObject chatter = null)
            : base(hud)
        {
            messages = new List<Message>();
            currentColor = Color.white;
            this.chatter = chatter;
            InitiateSprites();
        }

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
                    showCharacter = StripTags(CurrentMessage.text).Length;
                    showText = StripTags(CurrentMessage.text);
                }
                else if (showCharacter < StripTags(CurrentMessage.text).Length)
                {
                    showDelay++;
                    if (showDelay >= GetDelay())
                    {
                        showDelay = 0;
                        showCharacter++;
                        showText = StripTags(CurrentMessage.text).Substring(0, showCharacter);
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
                        }
                    }
                }

                if (ShowingAMessage && hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player && (hud.owner as Player).graphicsModule != null && (hud.owner as Player).abstractCreature.world.game.IsStorySession && (hud.owner as Player).abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
                {
                    ((hud.owner as Player).graphicsModule as PlayerGraphics).markBaseAlpha = Mathf.Min(1f, ((hud.owner as Player).graphicsModule as PlayerGraphics).markBaseAlpha + 0.005f);
                    ((hud.owner as Player).graphicsModule as PlayerGraphics).markAlpha = Mathf.Min(0.5f + UnityEngine.Random.value, 1f);
                }
            }
        
        
        }

        public void Interrupt(string text, int extraLinger)
        {
            if (messages.Count > 0)
            {
                messages = new List<Message> { messages[0] };
                lingerCounter = messages[0].linger + 1;
                showCharacter = messages[0].text.Length + 2;
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
                showCharacter = messages[0].text.Length + 2;
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
            label.text = StripTags(CurrentMessage.text);
            CreateCharactorForMessage(CurrentMessage);
            actualWidth = label.textRect.width;
            label.text = "";
        }
        public static string StripTags(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            return Regex.Replace(raw, @"</?(color|shake|rainbow|wave)[^>]*>", "", RegexOptions.IgnoreCase);
        }
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
        
        public void ClearCharactors()
        {
            foreach (var charactor in charactors)
            {
                hud.fContainers[1].RemoveChild(charactor);
            }
            charactors.Clear();
            charactorEffects.Clear();
        }

        public void CreateCharactorForMessage(Message msg)
        {
            ClearCharactors();

            var parsed = StyleParser.Parse(msg.text);

            foreach (var (c, style) in parsed)
            {
                var fl = new FLabel(Custom.GetFont(), c.ToString());
                charactors.Add(fl);

                var fx = new CharactorEffect(fl,label)
                {
                    Color = style.color,
                    ShakeIntensity = style.shake,
                    WaveIntensity = style.wave,
                    Rainbow = style.rainbow
                };
                charactorEffects.Add(fx);
            }

            foreach (var fl in charactors)
                hud.fContainers[1].AddChild(fl);
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].isVisible = CurrentMessage != null;
            }
            for (int i = 0; i < charactors.Count; i++)
            {
                charactors[i].isVisible = CurrentMessage != null;
            }
            label.isVisible = CurrentMessage != null;



            if (chatter != null && chatter.room == null)
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    sprites[i].isVisible = false;
                }
                for (int i = 0; i < charactors.Count; i++)
                {
                    charactors[i].isVisible = false;
                }
                label.isVisible = false;
            }

            if (CurrentMessage != null)
            {
                if (InGameTranslator.LanguageID.UsesLargeFont(hud.rainWorld.inGameTranslator.currentLanguage))
                {
                    _ = label.FontMaxCharWidth;
                }
                else
                {
                    _ = meanCharWidth;
                }

                float num = ((!InGameTranslator.LanguageID.UsesLargeFont(hud.rainWorld.inGameTranslator.currentLanguage)) ? lineHeight : label.FontLineHeight);
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
                label.color = currentColor;
                label.x = vector.x - actualWidth * 0.5f;
                label.y = vector.y + vector2.y / 2f - num * ((!InGameTranslator.LanguageID.UsesLargeFont(hud.rainWorld.inGameTranslator.currentLanguage)) ? 0.6666f : 0.3333f);
                label.text = showText;
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

                label.isVisible = false;
                for (int i = 0; i < charactors.Count; i++)
                {
                    if (label.text.Replace("\n", "").Length > i)
                    {
                        charactors[i].x = label.x + GetCharRelativeLinePosition(label, i).x;
                        charactors[i].y = label.y + GetCharRelativeLinePosition(label, i).y;
                        charactors[i].isVisible = true;
                    }
                    else
                    {
                        charactors[i].isVisible = false;

                    }
                }

                if (charactorEffects.Count > 0)
                {
                    foreach (var effect in charactorEffects)
                    {
                        if (effect.Owner != null)
                        {
                            effect.update();
                        }
                    }
                }

            }
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

            if (messages.Count > 0)
                InitNextMessage();
            else
                label.text = "";
        }

        public static Vector2 GetCharRelativeLinePosition(FLabel label, int charIndex)
        {
            if (label == null || label._letterQuadLines == null || charIndex < 0)
                return Vector2.zero;

            // 确保文本四边形是最新的
            if (label._doesTextNeedUpdate)
            {
                label.CreateTextQuads();
            }

            // 确保位置计算是最新的
            if (label._doesLocalPositionNeedUpdate)
            {
                label.UpdateLocalPosition();
            }

            int currentCharCount = 0;

            // 遍历所有行
            for (int lineIndex = 0; lineIndex < label._letterQuadLines.Length; lineIndex++)
            {
                FLetterQuadLine line = label._letterQuadLines[lineIndex];
                FLetterQuad[] quads = line.quads;

                // 检查指定字符是否在这一行
                if (charIndex < currentCharCount + quads.Length)
                {
                    // 找到目标字符在这个行中的索引
                    int quadIndex = charIndex - currentCharCount;
                    FLetterQuad quad = quads[quadIndex];

                    // 计算字符中心点的x坐标
                    float centerX = (quad.topLeft.x + quad.topRight.x) / 2f;

                    // 使用该行的基线y坐标（通常使用行的中间y值或根据字体基线调整）
                    // 这里我们可以使用该行bounds的中间值作为行位置
                    float lineY = (line.bounds.yMin + line.bounds.yMax) / 2f;

                    // 或者使用字体偏移（更准确）
                    float lineY2 = -line.bounds.y * label._anchorY; // 根据标签的UpdateLocalPosition逻辑

                    // 根据UpdateLocalPosition中的计算，行的y位置可以通过这种方式获取
                    // 从UpdateLocalPosition中可以看到，每行的y偏移是num6
                    // 我们需要重新计算num6
                    float minY = float.MaxValue;
                    float maxY = float.MinValue;

                    int lineCount = label._letterQuadLines.Length;
                    for (int i = 0; i < lineCount; i++)
                    {
                        FLetterQuadLine currentLine = label._letterQuadLines[i];
                        minY = Math.Min(currentLine.bounds.yMin, minY);
                        maxY = Math.Max(currentLine.bounds.yMax, maxY);
                    }

                    float lineOffsetY = 0f - (minY + (maxY - minY) * label._anchorY);

                    // 该行的实际y位置（相对于锚点）
                    float linePositionY = line.bounds.yMin + lineOffsetY + label._font.offsetY;

                    // 为了对齐整行，我们使用行位置而不是字符中心y
                    // 通常文本的基线在行中间偏下的位置，这里使用行的中间值
                    float finalLineY = linePositionY + line.bounds.height / 4f;

                    return new Vector2(centerX, finalLineY);
                }

                currentCharCount += quads.Length;
            }

            // 如果序号超出范围，返回最后一个字符的位置
            if (label._letterQuadLines.Length > 0)
            {
                FLetterQuadLine lastLine = label._letterQuadLines[label._letterQuadLines.Length - 1];
                if (lastLine.quads.Length > 0)
                {
                    FLetterQuad lastQuad = lastLine.quads[lastLine.quads.Length - 1];
                    float centerX = (lastQuad.topLeft.x + lastQuad.topRight.x) / 2f;

                    // 计算最后一行位置
                    float minY = float.MaxValue;
                    float maxY = float.MinValue;

                    for (int i = 0; i < label._letterQuadLines.Length; i++)
                    {
                        FLetterQuadLine currentLine = label._letterQuadLines[i];
                        minY = Math.Min(currentLine.bounds.yMin, minY);
                        maxY = Math.Max(currentLine.bounds.yMax, maxY);
                    }

                    float lineOffsetY = 0f - (minY + (maxY - minY) * label._anchorY);
                    float linePositionY = lastLine.bounds.yMin + lineOffsetY + label._font.offsetY;
                    float finalLineY = linePositionY/* + lastLine.bounds.height / 2f*/;

                    return new Vector2(centerX, finalLineY);
                }
            }

            return Vector2.zero;
        }

        public static class StyleParser
        {
            private static readonly Regex tagRegex = new Regex(
                @"<(/?)(color|shake|rainbow|wave)([^>]*)>", RegexOptions.IgnoreCase);

            public static List<(char c, CharStyle style)> Parse(string raw)
            {
                var list = new List<(char, CharStyle)>();
                var stack = new Stack<CharStyle>();   // 保存每一层标签进入前的状态
                var current = CharStyle.Default;
                stack.Push(current);

                int textStart = 0;
                var matches = tagRegex.Matches(raw);

                foreach (Match m in matches)
                {
                    // 1. 把标签之前的纯文本落盘
                    for (int i = textStart; i < m.Index; ++i)
                    {
                        if (raw[i] == '\n') continue;   // 换行符不生成字符，由 FLabel 处理
                        list.Add((raw[i], current));
                    }

                    bool isClose = m.Groups[1].Value == "/";
                    string tag = m.Groups[2].Value.ToLower();
                    string arg = m.Groups[3].Value;

                    if (!isClose)
                    {
                        // 进入标签：压栈并修改 current
                        stack.Push(current);
                        switch (tag)
                        {
                            case "color":
                                if (TryParseColor(arg.TrimStart('#'), out var c))
                                    current.color = c;
                                break;
                            case "shake":
                                if (float.TryParse(arg, out float s))
                                    current.shake = s;
                                break;
                            case "wave":
                                if (float.TryParse(arg, out float w))
                                    current.wave = w;
                                break;
                            case "rainbow":
                                current.rainbow = true;
                                break;
                        }
                    }
                    else
                    {
                        // 闭合标签：出栈恢复
                        if (stack.Count > 1) current = stack.Pop();
                    }

                    textStart = m.Index + m.Length;
                }

                // 尾部剩余文本
                for (int i = textStart; i < raw.Length; ++i)
                {
                    if (raw[i] == '\n') continue;
                    list.Add((raw[i], current));
                }

                return list;
            }

            private static bool TryParseColor(string hex, out Color c)
            {
                c = Color.white;
                if (uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out uint val))
                {
                    byte r = (byte)((val >> 24) & 0xFF);
                    byte g = (byte)((val >> 16) & 0xFF);
                    byte b = (byte)((val >> 8) & 0xFF);
                    byte a = (byte)(val & 0xFF);
                    c = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                    return true;
                }
                return false;
            }
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            ClearCharactors();
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].RemoveFromContainer();
            }

        }
    }
}
