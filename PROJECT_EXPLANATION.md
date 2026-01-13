# Giải Thích Chi Tiết Các Thành Phần Trong Project Super Mario Bros 2D

## 📁 Cấu Trúc Thư Mục

### Assets/
- **Materials/**: Vật liệu vật lý (NoFriction.physicsMaterial2D)
- **Prefabs/**: 27 prefab objects được sử dụng trong game
- **Scenes/**: Scene 1-1.unity (level đầu tiên)
- **Scripts/**: 20 C# scripts chứa logic game
- **Sprites/**: 45 sprite images cho game

---

## 🎮 Scripts - Giải Thích Chi Tiết

### 1. HỆ THỐNG PLAYER

#### `Player.cs` - Quản Lý Trạng Thái Player
**Chức năng chính:**
- Quản lý 2 kích thước: Small Mario và Big Mario
- Xử lý khi bị tấn công (Hit)
- Quản lý trạng thái chết (Death)
- Xử lý Starpower (bất tử tạm thời)

**Các thành phần:**
- `smallRenderer`: Sprite renderer cho Mario nhỏ
- `bigRenderer`: Sprite renderer cho Mario lớn
- `activeRenderer`: Renderer đang hoạt động
- `capsuleCollider`: Collider của player
- `movement`: Component di chuyển
- `deathAnimation`: Animation khi chết

**Các phương thức:**
- `Hit()`: Xử lý khi bị tấn công (shrink nếu big, death nếu small)
- `Death()`: Kích hoạt animation chết và reset level
- `Grow()`: Chuyển từ small sang big (tăng collider size)
- `Shrink()`: Chuyển từ big sang small (giảm collider size)
- `Starpower()`: Kích hoạt chế độ bất tử 10 giây với hiệu ứng màu sắc

**Logic đặc biệt:**
- ScaleAnimation: Nhấp nháy giữa small/big khi chuyển đổi
- StarpowerAnimation: Đổi màu ngẫu nhiên mỗi 4 frame trong 10 giây

---

#### `PlayerMovement.cs` - Hệ Thống Di Chuyển
**Chức năng chính:**
- Xử lý di chuyển ngang (trái/phải)
- Xử lý nhảy với physics tùy chỉnh
- Quản lý trọng lực và terminal velocity
- Phát hiện va chạm với ground và walls

**Các thuộc tính:**
- `moveSpeed`: Tốc độ di chuyển (8f)
- `maxJumpHeight`: Chiều cao nhảy tối đa (5f)
- `maxJumpTime`: Thời gian nhảy tối đa (1f)
- `jumpForce`: Lực nhảy (tính toán từ height/time)
- `gravity`: Trọng lực tùy chỉnh (tính toán từ height/time)

**Các trạng thái:**
- `grounded`: Đang đứng trên mặt đất
- `jumping`: Đang nhảy
- `running`: Đang chạy (velocity > 0.25f)
- `sliding`: Đang trượt (đổi hướng đột ngột)
- `falling`: Đang rơi

**Các phương thức:**
- `HorizontalMovement()`: Xử lý di chuyển ngang với acceleration/deceleration
- `GroundedMovement()`: Xử lý nhảy khi đứng trên mặt đất
- `ApplyGravity()`: Áp dụng trọng lực (x2 khi falling)
- `OnCollisionEnter2D()`: Xử lý va chạm với enemy và walls

**Đặc điểm:**
- Sử dụng custom gravity thay vì Unity default
- Raycast để phát hiện va chạm thay vì OnCollision
- Clamp position trong screen bounds
- Bounce off enemy head khi đạp lên

---

#### `PlayerSpriteRenderer.cs` - Quản Lý Sprite Player
**Chức năng:**
- Tự động thay đổi sprite dựa trên trạng thái movement
- Quản lý animation run

**Các sprite:**
- `idle`: Sprite khi đứng yên
- `jump`: Sprite khi nhảy
- `slide`: Sprite khi trượt
- `run`: AnimatedSprite cho animation chạy

**Logic:**
- `LateUpdate()`: Kiểm tra trạng thái movement và cập nhật sprite
- Ưu tiên: jump > slide > run > idle

---

### 2. HỆ THỐNG ENEMY

#### `EntityMovement.cs` - Di Chuyển Entity Cơ Bản
**Chức năng:**
- Di chuyển tự động theo hướng (direction)
- Tự động đổi hướng khi chạm tường
- Tối ưu: chỉ hoạt động khi trong camera view

**Các thuộc tính:**
- `speed`: Tốc độ di chuyển (1f)
- `direction`: Hướng di chuyển (Vector2.left mặc định)

**Tối ưu hiệu năng:**
- `OnBecameVisible()`: Kích hoạt khi vào camera view
- `OnBecameInvisible()`: Tắt khi ra khỏi camera view
- Sleep Rigidbody khi disabled

**Logic:**
- Sử dụng Physics2D.gravity cho trọng lực
- Raycast để phát hiện va chạm và đổi hướng
- Tự động xoay sprite theo hướng di chuyển

---

#### `Goomba.cs` - Enemy Goomba
**Chức năng:**
- Xử lý va chạm với player
- Có thể bị dẹp (Flatten) hoặc bị tiêu diệt (Hit)

**Các trường hợp:**
1. **Player đạp lên đầu**: `Flatten()` - dẹp và destroy sau 0.5s
2. **Player chạm bên cạnh**: `player.Hit()` - làm player bị thương
3. **Starpower**: `Hit()` - bị tiêu diệt ngay lập tức
4. **Shell đánh trúng**: `Hit()` - bị tiêu diệt

**Phương thức Flatten:**
- Tắt collider và EntityMovement
- Tắt AnimatedSprite
- Đổi sang flatSprite
- Destroy sau 0.5 giây

**Phương thức Hit:**
- Tắt AnimatedSprite
- Kích hoạt DeathAnimation
- Destroy sau 3 giây

---

#### `Koopa.cs` - Enemy Koopa (Rùa)
**Chức năng:**
- Có 2 trạng thái: Normal và Shell
- Có thể đẩy shell để tấn công

**Các trạng thái:**
- `shelled`: Đã vào shell
- `pushed`: Shell đã được đẩy

**Các trường hợp:**
1. **Player đạp lên đầu (chưa shell)**: `EnterShell()` - vào shell
2. **Player chạm bên cạnh (chưa shell)**: `player.Hit()`
3. **Player chạm shell (chưa push)**: `PushShell()` - đẩy shell
4. **Player chạm shell (đã push)**: `player.Hit()` hoặc `Hit()` nếu starpower
5. **Shell đánh trúng (chưa shell)**: `Hit()` - bị tiêu diệt

**Phương thức EnterShell:**
- Đổi sprite sang shellSprite
- Tắt AnimatedSprite và EntityMovement
- Set shelled = true

**Phương thức PushShell:**
- Set pushed = true
- Kích hoạt EntityMovement với speed cao (12f)
- Đổi layer sang "Shell" để có thể tiêu diệt enemy khác
- Direction dựa trên vị trí player

**Tối ưu:**
- Destroy shell khi ra khỏi camera view

---

### 3. HỆ THỐNG POWER-UP

#### `PowerUp.cs` - Quản Lý Power-Up
**Chức năng:**
- Xử lý thu thập các loại power-up khác nhau

**Các loại Power-Up:**
- `Coin`: Thêm coin vào GameManager
- `ExtraLife`: Thêm 1 mạng
- `MagicMushroom`: Làm player lớn lên (Grow)
- `Starpower`: Kích hoạt chế độ bất tử

**Logic:**
- `OnTriggerEnter2D()`: Phát hiện khi player chạm vào
- `Collect()`: Xử lý tùy theo type và destroy object

---

#### `BlockItem.cs` - Item Từ Block
**Chức năng:**
- Animation khi item xuất hiện từ block

**Quy trình:**
1. Ẩn sprite và tắt physics trong 0.25s
2. Hiện sprite và di chuyển lên trên trong 0.5s
3. Kích hoạt physics để item rơi xuống

**Các component:**
- Rigidbody2D: Physics
- CircleCollider2D: Physics collider
- BoxCollider2D: Trigger collider
- SpriteRenderer: Hiển thị

---

#### `BlockCoin.cs` - Coin Từ Block
**Chức năng:**
- Animation coin bay lên và biến mất

**Quy trình:**
1. Thêm coin vào GameManager ngay khi Start
2. Animation bay lên 2 đơn vị
3. Animation rơi xuống
4. Destroy object

**Thời gian:**
- Mỗi animation: 0.25 giây

---

### 4. HỆ THỐNG LEVEL ELEMENTS

#### `BlockHit.cs` - Xử Lý Block Bị Đập
**Chức năng:**
- Xử lý khi player đập block từ dưới lên
- Spawn item nếu có
- Animation block nhảy lên

**Các thuộc tính:**
- `item`: GameObject sẽ spawn khi đập (null nếu không có)
- `emptyBlock`: Sprite khi block đã hết hit
- `maxHits`: Số lần có thể đập (-1 = vô hạn)

**Logic:**
- Chỉ hoạt động khi đập từ dưới lên (DotTest với Vector2.up)
- `animating`: Flag để tránh đập nhiều lần cùng lúc
- Animation: Di chuyển lên 0.5 đơn vị rồi về vị trí cũ

**Quy trình:**
1. Kiểm tra điều kiện (không animating, maxHits != 0)
2. Giảm maxHits
3. Đổi sprite nếu maxHits == 0
4. Spawn item nếu có
5. Chạy animation

---

#### `FlagPole.cs` - Cột Cờ Kết Thúc Level
**Chức năng:**
- Xử lý khi player chạm vào cột cờ
- Animation hoàn thành level
- Chuyển sang level tiếp theo

**Các thuộc tính:**
- `flag`: Transform của cờ (di chuyển xuống)
- `poleBottom`: Vị trí dưới cột cờ
- `castle`: Vị trí lâu đài
- `speed`: Tốc độ di chuyển (6f)
- `nextWorld`, `nextStage`: Level tiếp theo

**Quy trình LevelCompleteSequence:**
1. Tắt PlayerMovement
2. Di chuyển flag xuống dưới
3. Di chuyển player xuống dưới cột cờ
4. Di chuyển player sang phải
5. Di chuyển player xuống dưới
6. Di chuyển player vào lâu đài
7. Ẩn player
8. Đợi 2 giây
9. Load level tiếp theo

**Phương thức MoveTo:**
- Di chuyển Transform đến vị trí với tốc độ cố định
- Sử dụng MoveTowards

---

#### `Pipe.cs` - Ống Nước (Pipe)
**Chức năng:**
- Cho phép player di chuyển giữa các khu vực
- Hỗ trợ underground

**Các thuộc tính:**
- `connection`: Pipe kết nối
- `enterKeyCode`: Phím để vào (S mặc định)
- `enterDirection`: Hướng vào (down mặc định)
- `exitDirection`: Hướng ra (zero = teleport)

**Quy trình Enter:**
1. Kiểm tra player nhấn phím S
2. Tắt PlayerMovement
3. Di chuyển player vào pipe (scale 0.5)
4. Đợi 1 giây
5. Điều chỉnh camera nếu underground
6. Teleport hoặc di chuyển player đến connection
7. Bật lại PlayerMovement

**Camera:**
- Tự động điều chỉnh camera height nếu underground

---

#### `DeathBarrier.cs` - Rào Chắn Chết
**Chức năng:**
- Phát hiện khi player hoặc object rơi xuống vực
- Reset level hoặc destroy object

**Logic:**
- `OnTriggerEnter2D()`: Phát hiện khi chạm vào
- Nếu là Player: SetActive(false) và ResetLevel
- Nếu là object khác: Destroy ngay

---

### 5. GAME MANAGEMENT

#### `GameManager.cs` - Quản Lý Game
**Chức năng:**
- Singleton quản lý toàn bộ game state
- Quản lý lives, coins, world, stage
- Xử lý load level, reset level, game over

**Các thuộc tính:**
- `world`: World hiện tại (1)
- `stage`: Stage hiện tại (1)
- `lives`: Số mạng (3)
- `coins`: Số coin (0)

**Các phương thức:**
- `NewGame()`: Bắt đầu game mới (reset lives, coins, load level 1-1)
- `GameOver()`: Kết thúc game (gọi NewGame)
- `LoadLevel(world, stage)`: Load scene theo format "{world}-{stage}"
- `NextLevel()`: Load stage tiếp theo
- `ResetLevel(delay)`: Reset level sau delay
- `ResetLevel()`: Reset level ngay (giảm lives, load lại hoặc game over)
- `AddCoin()`: Thêm coin (100 coins = 1 life)
- `AddLife()`: Thêm 1 mạng

**Singleton Pattern:**
- `Instance`: Static instance
- `DontDestroyOnLoad`: Giữ lại khi chuyển scene
- `DefaultExecutionOrder(-1)`: Chạy trước các script khác

**Settings:**
- `Application.targetFrameRate = 60`: Giới hạn FPS

---

### 6. CAMERA & UTILITIES

#### `SideScrollingCamera.cs` - Camera Side-Scrolling
**Chức năng:**
- Camera theo dõi player theo chiều ngang
- Hỗ trợ underground camera

**Các thuộc tính:**
- `trackedObject`: Object cần theo dõi (Player)
- `height`: Chiều cao camera bình thường (6.5f)
- `undergroundHeight`: Chiều cao camera underground (-9.5f)
- `undergroundThreshold`: Ngưỡng để xác định underground (0f)

**Logic:**
- `LateUpdate()`: Cập nhật vị trí camera sau khi player di chuyển
- Chỉ di chuyển theo chiều ngang (x), không lùi lại
- `SetUnderground()`: Điều chỉnh chiều cao camera

---

#### `AnimatedSprite.cs` - Animation Sprite
**Chức năng:**
- Tự động chuyển đổi giữa các sprite trong mảng

**Các thuộc tính:**
- `sprites`: Mảng các sprite để animate
- `framerate`: Tốc độ animation (1f/6f = 6 FPS mặc định)

**Logic:**
- `InvokeRepeating()`: Gọi Animate() theo framerate
- Tự động loop (frame = 0 khi hết mảng)
- Tắt khi component disabled

---

#### `DeathAnimation.cs` - Animation Chết
**Chức năng:**
- Animation khi player hoặc enemy chết
- Nhảy lên và rơi xuống

**Các thuộc tính:**
- `spriteRenderer`: Sprite renderer
- `deadSprite`: Sprite khi chết (null = giữ nguyên)

**Quy trình:**
1. `OnEnable()`: Cập nhật sprite, tắt physics, bắt đầu animation
2. `UpdateSprite()`: Set sprite và sorting order
3. `DisablePhysics()`: Tắt tất cả colliders và movement
4. `Animate()`: Nhảy lên với velocity và rơi xuống với gravity

**Physics:**
- Jump velocity: 10f
- Gravity: -36f
- Duration: 3 giây

---

#### `Extensions.cs` - Extension Methods
**Chức năng:**
- Cung cấp các phương thức mở rộng hữu ích

**Raycast (Rigidbody2D):**
- Kiểm tra va chạm theo hướng cụ thể
- Sử dụng ClosestPoint và OverlapCircle
- Trả về true nếu có collider khác trong hướng đó

**DotTest (Transform):**
- Kiểm tra transform có đang hướng về transform khác không
- Sử dụng Vector2.Dot
- Threshold: 0.25f
- Ví dụ: Kiểm tra player có đạp lên enemy không (Vector2.down)

---

## 🎨 Prefabs - Các Object Trong Game

### Player & Enemies
- **Mario.prefab**: Player character với tất cả components
- **Goomba.prefab**: Enemy Goomba
- **Koopa.prefab**: Enemy Koopa

### Blocks
- **Brick.prefab**: Block gạch có thể phá
- **HardBlock.prefab**: Block cứng không thể phá
- **MysteryBlock.prefab**: Block bí ẩn có item
- **EmptyBlock.prefab**: Block đã hết item
- **UndergroundBlock.prefab**: Block cho underground
- **UndergroundBrick.prefab**: Gạch cho underground

### Power-Ups
- **Coin.prefab**: Coin có thể thu thập
- **MagicMushroom.prefab**: Nấm làm lớn
- **1upMushroom.prefab**: Nấm thêm mạng
- **Starman.prefab**: Sao bất tử
- **BlockCoin.prefab**: Coin từ block

### Level Elements
- **Flag Pole.prefab**: Cột cờ kết thúc level
- **Castle.prefab**: Lâu đài
- **Pipe.prefab**: Ống nước
- **PipeConnection.prefab**: Điểm kết nối pipe
- **Ground.prefab**: Mặt đất

### Decorative Elements
- **Bush1/2/3.prefab**: Cây bụi trang trí
- **Cloud1/2/3.prefab**: Mây trang trí
- **Hill1/2.prefab**: Đồi trang trí

---

## ⚙️ Project Settings

### Packages
- **Unity 2D Feature**: Hỗ trợ 2D
- **TextMeshPro**: Text rendering
- **Visual Studio Integration**: IDE support
- **Physics2D Module**: Vật lý 2D

### Scene Structure
- **1-1.unity**: Level đầu tiên (World 1, Stage 1)
- Scene naming: "{world}-{stage}.unity"

---

## 🔄 Luồng Hoạt Động Game

### 1. Khởi Động
1. GameManager khởi tạo (Singleton)
2. Set targetFrameRate = 60
3. Gọi NewGame() → LoadLevel(1, 1)

### 2. Gameplay
1. Player di chuyển và nhảy
2. Va chạm với enemy → Xử lý theo logic
3. Thu thập power-up → Áp dụng effect
4. Đập block → Spawn item
5. Chạm cột cờ → Hoàn thành level

### 3. Kết Thúc Level
1. Player chạm FlagPole
2. Animation hoàn thành
3. GameManager.LoadLevel(nextWorld, nextStage)

### 4. Player Chết
1. Player.Hit() → Death()
2. DeathAnimation chạy
3. GameManager.ResetLevel(3f)
4. Giảm lives
5. Load lại level hoặc GameOver

---

## 🎯 Các Tính Năng Đặc Biệt

### 1. Custom Physics
- Gravity và jump force được tính toán từ maxJumpHeight và maxJumpTime
- Terminal velocity được giới hạn
- Raycast thay vì OnCollision để tối ưu

### 2. Performance Optimization
- Entity chỉ hoạt động khi trong camera view (OnBecameVisible)
- Rigidbody sleep khi không cần thiết
- Singleton pattern cho GameManager

### 3. Animation System
- AnimatedSprite cho sprite animation
- Coroutine cho các animation phức tạp
- Scale animation khi chuyển đổi size

### 4. Extension Methods
- Raycast và DotTest giúp code gọn và dễ đọc
- Tái sử dụng logic va chạm

---

## 📝 Ghi Chú Quan Trọng

1. **Layer System**: Sử dụng layers "Enemy", "PowerUp", "Shell", "Default"
2. **Tag System**: Sử dụng tag "Player" cho player
3. **Scene Naming**: Phải theo format "{world}-{stage}" để LoadLevel hoạt động
4. **Physics Material**: NoFriction cho một số object
5. **Execution Order**: GameManager có DefaultExecutionOrder(-1) để chạy trước

---

## 🔧 Cấu Hình Cần Thiết

### Player (Mario.prefab)
- Tag: "Player"
- Layer: Default
- Components:
  - Player
  - PlayerMovement
  - PlayerSpriteRenderer (2 instances: small, big)
  - DeathAnimation
  - CapsuleCollider2D
  - Rigidbody2D

### Enemy
- Layer: Enemy
- Components:
  - EntityMovement
  - AnimatedSprite
  - DeathAnimation
  - Collider2D
  - Rigidbody2D

### Power-Up
- Layer: PowerUp
- Components:
  - PowerUp
  - Collider2D (Trigger)
  - SpriteRenderer

---

Đây là tài liệu giải thích đầy đủ về tất cả các thành phần trong project Super Mario Bros 2D. Mỗi script đều có vai trò và chức năng cụ thể, tạo nên một hệ thống game hoàn chỉnh và mạch lạc.
