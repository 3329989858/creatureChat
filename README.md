# creatureChat
A Rainworld mod that can create a dialog box that follows any item.

## Quick Start

### Basic Dialogue

```csharp
// Create dialogue in any creature AI
new CreatureChatTx(
    room,           // Current room
    0,              // Frames to wait before starting
    player,         // Speaking character
    "Hello World!"  // Dialogue content
);
