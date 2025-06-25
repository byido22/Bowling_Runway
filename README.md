# 🎳 Bowling_Runway

**Bowling_Runway** is a C# Windows Forms application that simulates a bowling game with custom textures and sounds.

## 🧩 Features
- Custom 3D textures and images for walls, floor, and lane
- Ball and pin models with texture loading
- Sound effects for pin hits and throws
- Interactive gameplay experience

## 🖥️ Technologies
- C# with Windows Forms (.NET)
- OpenGL 
- Resource loading via file paths

## 📁 Project Structure

```
Bowling_Runway/
├── Bowling_Runway.sln
├── Bowling_Runway/
│   ├── Form1.cs
│   ├── Program.cs
│   ├── bin/
│   │   └── Debug/
│   │       └── pic/
│   │           ├── wood.jpg
│   │           ├── ball1.jpg
│   │           └── ...
```

> ⚠️ **Note:** Textures and sound files are located inside `bin/Debug/pic` to match the hardcoded file paths in the code.

## 🚀 How to Run

1. Clone or download this repository:
   ```bash
   git clone https://github.com/byido22/Bowling_Runway.git
   ```

2. Open the solution file (`Bowling_Runway.sln`) in Visual Studio.

3. Build the project in **Debug** mode.

4. Run the application – textures and sound should load automatically from `bin/Debug/pic`.

## 📜 License
This project is for educational or personal portfolio use. No license applied.
