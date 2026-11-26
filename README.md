# 🗡️ Hiển Ngu Phiêu Lưu Kí

Một tựa game nhập vai hành động góc nhìn từ trên xuống (**2D Top-Down Action RPG**) được phát triển bằng **Unity 6 (`6000.6.9f1`)**. Game kết hợp lối chơi chiến đấu thời gian thực tốc độ cao, hệ thống vũ khí cận chiến sắc bén và hành trình sinh tồn đầy thử thách trong thế giới 2.5D sống động.

---

# 🗡️ Hiển Ngu Phiêu Lưu Kí

> *"IQ có thể thấp, nhưng độ ngáo phải chạm nóc vũ trụ."*

---

## 📖 Giới thiệu trò chơi (About the Game)

**Hiển Ngu Phiêu Lưu Kí** đưa người chơi nhập vai vào **Hiển** — một *"kỳ tài"* trong làng thiếu muối với chỉ số IQ luôn nằm ở mức báo động đỏ, nhưng độ ngáo thì chạm nóc vũ trụ. 

* **Không** siêu năng lực.
* **Không** hệ thống gian lận.
* Thứ duy nhất Hiển sở hữu là khuôn mặt đậm chất **"ngáo ngơ"** đến mức quái vật nhìn cũng phải ngỡ ngàng bật ngửa vì tưởng đồng loại. 

Vô tình bị ném vào vùng đất đầy rẫy sinh vật biến dị, hành trình của Hiển thực chất là một chuỗi tai nạn nghề nghiệp nối tiếp nhau, nơi cậu ta phải gồng mình gánh còng lưng... sự ngốc nghếch của chính bản thân để không bị tiễn đi *"đăng xuất"* sớm.

---

## 🌲 Cốt truyện & Bối cảnh (Story & Setting)

Khởi đầu với cái đầu rỗng tuếch cùng đôi chân chạy trốn nhanh hơn cả tốc độ ánh sáng, Hiển buộc phải học cách sử dụng thanh kiếm trên tay — dù với tư duy của cậu ta, việc phân biệt **cán kiếm** với **lưỡi kiếm** đã là một thành tựu khoa học vĩ đại. 

Người chơi sẽ phải *thay Hiển động não*, quan sát sơ hở của kẻ thù và thực hiện những pha lướt né *"chân nam đá chân chiêu"* nhưng đầy tính nghệ thuật để giữ lại cái mạng quèn. 

Băng qua những *khu rừng âm u nơi ánh nắng còn hiếm hoi hơn cả nếp nhăn trên não Hiển*, bạn sẽ chạm trán bầy **Blue Slime** — những cục thạch màu xanh trông thì vô tri giống hệt nhân vật chính, nhưng lại dư sức *"vả"* cho Hiển tỉnh ngộ mỗi khi cậu ta nổi máu anh hùng rơm.

### Lối chơi (Gameplay)
- **Hành động thời gian thực (Real-time Action):** Không có lượt đánh chờ đợi — mọi pha vung kiếm, né đòn và phản công đều diễn ra liên tục, đòi hỏi sự nhạy bén và căn thời điểm chuẩn xác.
- **Nhịp độ nhanh & Linh hoạt:** Kết hợp nhuần nhuyễn giữa di chuyển 8 hướng, kỹ năng lướt nhanh (**Dash**) để thoát vòng vây hoặc tiếp cận mục tiêu chớp nhoáng.
- **Phản hồi chiến đấu (Combat Feel):** Mỗi nhát chém trúng đích đều đi kèm hiệu ứng vệt kiếm (`Slash VFX`), lực đẩy lùi (`Knockback`) và hiệu ứng chớp sáng (`Flash`), mang lại cảm giác va chạm chân thực và đã tay.

---

## 🎮 Tính năng nổi bật

- **Di chuyển & Điều khiển mượt mà (Player Movement):** Di chuyển 8 hướng linh hoạt tích hợp **Unity New Input System**, hỗ trợ kỹ năng lướt nhanh (**Dash**) kèm bộ hoạt ảnh (Animation) đồng bộ theo từng hướng nhìn của nhân vật.
- **Hệ thống Chiến đấu Cận chiến (Melee Combat):** Trang bị kiếm (`Sword`), tạo hiệu ứng vệt chém (`Slash VFX`), nhận diện vùng va chạm chính xác qua kiến trúc interface `IDamageable`.
- **AI Quái vật thông minh (Enemy AI - Blue Slime):** Tự động tuần tra khu vực (`Roaming`), phát hiện tầm nhìn và truy đuổi người chơi (`Chasing`), thực hiện đòn tấn công (`Attacking`), chịu hiệu ứng đẩy lùi (`Knockback`), nháy sáng khi trúng chiêu (`Flash`) và biến mất khi cạn máu.
- **Đồ họa & Chiều sâu 2.5D (2.5D Depth Sorting):** Tự động tính toán và sắp xếp thứ tự hiển thị theo trục Y (`sortingOrder = -y * 10`) kết hợp **URP 2D**, tạo chiều sâu tự nhiên khi nhân vật di chuyển xuyên qua tán cây và vật thể trong môi trường.

---

## 🛠️ Công nghệ & Kiến trúc dự án

- **Engine:** Unity `6000.6.9f1` (Universal Render Pipeline - URP 2D)
- **Input:** Unity Input System (`activeInputHandler: 1`)
- **Cấu trúc mã nguồn (`Assets/Scripts/`):**
  - `Player/`: `PlayerController` (Singleton), `PlayerCombat`, `PlayerHealth`, `MeleeHitbox`.
  - `Enemies/`: `EnemyAI`, `EnemyPathfinding`, `Slime` (`IEnemy`), `EnemyHealth`.
  - `Combat / Shared/`: `IDamageable`, `IEnemy`, `Knockback`, `Flash`.
  - `Editor/`: Các công cụ tự động dựng map và thiết lập prefab (`MapSetup.cs`, `SlimeSetup.cs`).

---

## 🚀 Hướng dẫn cài đặt & Chạy thử

1. **Clone repository:**
   ```bash
   git clone https://github.com
   cd HienNguPhieuLuuKi
   ```
2. **Mở dự án bằng Unity Hub:**
   - Chọn **Add** -> trỏ tới thư mục dự án vừa clone.
   - Đảm bảo chọn đúng phiên bản **Unity 6 (`6000.6.9f1`)**.
3. **Chạy game:**
   - Mở scene chính tại `Assets/Scenes/SampleScene.unity`.
   - Nhấn nút **Play** trên Unity Editor để trải nghiệm.

---

## 📝 Giấy phép (License)

Dự án được phát triển phục vụ mục đích học tập và nghiên cứu lập trình game với Unity.
