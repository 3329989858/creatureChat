# creatureChat
A Rainworld mod that can create a dialog box that follows any item.

## Quick Start

### Basic Dialogue
This class can be created anywhere, and once it is created, it will automatically operate,This is the simplest example:

```csharp
new CreatureChatTx(
    room,           // Current room
    0,              // Frames to wait before starting
    player,         // Speaking character, if this is null, the dialogue will show in the buttom of the screen
    "I'm a slugpup,meow!"  // Dialogue content
);
```

### Multi-part Dialogue
Use <NEXT> to separate multiple dialogue segments:

```csharp
new CreatureChatTx(
    room,           // Current room
    0,              // Frames to wait before starting
    player,         // Speaking character
    "I'm a slugpup,meow!<NEXT>I said, meow!"  // Dialogue content
);
```

###Dialogue with Screen Prompts

```csharp
new CreatureChatTx(room, 0, player, "I'm a slugpup,meow!",
    textprompt: "Screen prompt text",    // Text displayed at bottom of screen
    textPromptWait: 10,                  // Prompt wait time
    textPromptTime: 320);                // Prompt display duration
```

###Interruptible Dialogue

```csharp
new CreatureChatTx(room, 0, player, "Dialogue content",
    canInterruptedByStun: true,   // Interrupt when stunned
    canInterruptedByDead: true);  // Interrupt when dead
```

## Text Formatting

| Tag | Function | Example |
|-----|----------|---------|
| `<LINE>` | Line break in text | `"First line<LINE>Second line"` |
| `<NEXT>` | Separate dialogue segments | `"Segment 1<NEXT>Segment 2"` |****


## Parameter Reference

| Parameter | Type | Description |
|-----------|------|-------------|
| `room` | `Room` | Current game room |
| `preWaitCounter` | `int` | Frames to wait before dialogue starts |
| `chatter` | `Player` | Speaking character |
| `talkText` | `string` | Dialogue content |
| `canInterruptedByStun` | `bool` | Whether to interrupt when stunned |
| `canInterruptedByDead` | `bool` | Whether to interrupt when dead |
| `textprompt` | `string` | Screen prompt text |
| `textPromptWait` | `int` | Prompt wait time |
| `textPromptTime` | `int` | Prompt display duration |

