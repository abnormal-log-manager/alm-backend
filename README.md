# Shortlink API Backend Documentation
## 📚 Mục lục
- [Shortlink API](#-shortlink-api)
  - [Tổng quan](#-tổng-quan)
  - [1. API tạo Short URL](#1-api-tạo-short-url)
  - [2. Bulk Create](#2-bulk-create)
  - [3. Get All URLs (Paging)](#3-get-all-urls-paging)
  - [4. Get URL by ID](#4-get-url-by-id)
  - [5. Delete Permanently](#5-delete-permanently)
  - [6. Soft Delete](#6-soft-delete)
  - [7. Search URL](#7-search-url)
  - [8. Filter URLs](#8-filter-urls)
  - [9. Update Short URL](#9-update-short-url)
  - [10. Export to Excel](#10-export-to-excel)
  - [11. Import from Excel](#11-import-from-excel)
  - [12. Redirect](#12-redirect)
  - [13. Statistics per Team & Level](#13-statistics-per-team--level)
- [Shortlink Repository](#shortlink-repository)
  - [Tổng quan](#-tổng-quan-1)
  - [1. Constructor](#1-constructor)
  - [2. GetByOriginalUrlAsync](#2-getbyoriginalurlasync)
  - [3. GetByShortCodeAsync](#3-getbyshortcodeasync)
  - [4. GetPaginatedAsync](#4-getpaginatedasync)
  - [5. GetFilteredAsync](#5-getfilteredasync)
  - [6. GetStatsPerTeamAsync](#6-getstatsperteamasync)
  - [7. SearchAsync](#7-searchasync)
  - [8. ExistsByShortCodeAsync](#8-existsbyshortcodeasync)
  - [9. GenerateTitleFromUrl](#9-generatetitlefromurl)
  - [10. ExportToExcelAsync](#10-exporttoexcelasync)
  - [11. ImportFromExcelAsync](#11-importfromexcelasync)
- [Document chi tiết](#document-chi-tiết)

---
# 📘 Shortlink API

# 📘 ShortURL Repository Documentation

---

## 🔗 Tổng quan

Hệ thống API phục vụ cho việc rút gọn link nội bộ, phục vụ quản lý log lỗi và phân tích.

---

## 1. API tạo Short URL

### Endpoint:

```csharp
POST /api/ShortUrl
```

### Request Body (JSON)

```json
{
  "originalUrl": "https://siemdc-stg.sendo.vn/en-US/app/search/search?q=log-queries",
  "team": "TMS",
  "level": "Error"
}
```

### Response (result)

```json
{
  "id": 123,
  "originalUrl": "https://siemdc-stg.sendo.vn/en-US/app/search/search?q=log-queries",
  "shortenedUrl": "https://alm-test.sendo.vn/api/r/abc12345",
  "team": "TMS",
  "level": "Error",
  "createDate": "2025-07-01T10:45:00Z"
}
```

### Luồng xử lý:

1. **[ShortUrlController]** nhận request từ client tại `/api/ShortUrl`.
2. Chờ `_service.ShortenUrlAsync(vm)` trong **`ShortUrlService`** tạo mới dữ liệu sau đó gán vào `result` 
3. **ShortenUrlAsync(vm)**:
    - Kiểm tra xem `originalUrl` đã tồn tại trong DB chưa:
        - ❌ Nếu có → throw exception “This URL already exists.”.
        - ✅ Nếu chưa → tạo mới:
            - Generate short code: `Guid.NewGuid().ToString("N").Substring(0, 8)`
            - Generate title:`var generatedTitle = await GenerateTitleFromUrl(entity.OriginalUrl);`
            - Gán `shortenedUrl = BaseDomain + /r/ + shortCode`
            - Lưu vào DB thông qua `_repo.CreateAsync(entity)`
            - Lưu tất cả thay đổi vào DB với `SaveChangesAsync()`.
            - Trả về `ShortUrlVM` đã được map từ Entity.
4. Trả về `result` 

---

## 2. Bulk Create

### Endpoint

```
POST /api/ShortUrl/bulk
```

### Request Body

```json
[
  { "originalUrl": "...", "team": "...", "level": "..." },
  ...
]
```

### Response(results)

```json
[
  { "id": 1, "originalUrl": "...", ... },
  ...
]
```

### Luồng xử lý:

1. **[ShortUrlController]** nhận request từ client tại `/api/ShortUrl/bulk`.
2. Chờ `_service.ShortenUrlBulkAsync(*vms*)` trong **`ShortUrlService`** tạo mới ****sau đó gán vào `results`
3. **ShortenUrlBulkAsync(vms)**: Tạo một vòng lặp, trong đó
    - Kiểm tra từng URL → nếu trùng thì skip.
    - Nếu mới thì tạo entity mới.
    - Sau khi kết thúc vòng lặp, lưu tất cả thay đổi vào DB với `SaveChangesAsync()`.
4. Trả về `result` 

---

## 3. Get All URLs (Paging)

### Endpoint

```
GET /api/ShortUrl?page=1&pageSize=10
```

### Response

```json
{
  "totalItems": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10,
  "data": [ ... ]
}
```

### Luồng xử lý:

1. **[ShortUrlController]** nhận request từ client tại `/api/ShortUrl`
2. Gọi `_service.ReadAllAsync(*page*, *pageSize*)` trong **`ShortUrlService`**
3. **ReadAllAsync(page, pageSize):**
    - Gọi đến repository `_repo.GetPaginatedAsync(page, pageSize)` để lấy dữ liệu từ database đã được phân trang.
        - `items` là danh sách các thực thể `ShortUrl` lấy được.
        - `totalCount` là tổng số bản ghi phù hợp với điều kiện (thường dùng để hiển thị tổng số trang trong UI).
    - Dùng `AutoMapper` để chuyển danh sách các thực thể (`ShortUrl`) thành danh sách các ViewModel (`ShortUrlVM`).
    - Trả về tuple chứa danh sách đã map (`result`) và tổng số bản ghi (`totalCount`).
4. Trả về `response`

---

## 4. Get URL by ID

### Endpoint

```
GET /api/ShortUrl/{id}
```

### Response(url)

```json
{ "id": 1, "originalUrl": "...", ... }
```

### Luồng xử lý:

1. **[ShortUrlController]** nhận request từ client tại `/api/ShortUrl/{id}` 
2. Chờ `_service.ReadAsync(*id*)` trong **`ShortUrlService`** để gán vào `url` 
3. **ReadAsync(id):**
    - Gọi đến repository `_unit.ShortUrlRepo.ReadAsync(id)`
    - Dùng `AutoMapper` để chuyển thực thể (`ShortUrl`) thành ViewModel (`ShortUrlVM`).
    - Trả về `ShortUrlVM` được gán thành `result`
4. Trả về `url` 

---

## 5. Delete Permanently

### Endpoint

```
DELETE /api/ShortUrl/{id}
```

### Response

```json
"Url deleted"
```

### **Luồng xử lý:**

1. **[ShortUrlController]** nhận request từ client tại `/api/ShortUrl/{id}` 
2. Gọi `_service.DeleteAsync(id)` trong **`ShortUrlService`** 
3. DeleteAsync(id):
    - Chờ `_unit.ShortUrlRepo.ReadAsync(id)` hoàn tất lấy dữ liệu ghi theo `(id)`
    - Gọi `_unit.ShortUrlRepo.DeleteAsync(id)` để xóa dữ liệu đó
    - Lưu tất cả thay đổi vào DB với `SaveChangeAsync()`
4. Trả về status `Ok(”Url soft deleted”);` 

---

## 6. Soft Delete

### Endpoint

```
DELETE /api/ShortUrl/softdel?id=123
```

### Response

```json
"Url soft deleted"
```

### Luồng xử lý:

1. [ShortUrlController] nhận request tại /api/ShortUrl/softdel/{id}
2. Gọi `_service.SoftDeleteAsync(id)` trong **`ShortUrlService`**
3. **SoftDeleteAsync(id):**
    - Chờ `_unit.ShortUrlRepo.ReadAsync(id)` để lấy bản ghi theo `(id)` sau đó gán vào `record`
    - Gọi `_unit.ShortUrlRepo.SoftDelete(record)`
    - Lưu tất cả thay đổi vào trong DB với `SaveChangeAsync()`
4. Trả về status `Ok(”Url soft deleted”);` 

---

## 7. Search URL

### Endpoint

```
GET /api/ShortUrl/search?query=abc12345
```

### Response(result)

```json
{ "id": 1, "originalUrl": "...", ... }
```

### **Luồng xử lý:**

1. **[ShortUrlController]** nhận request tại `/api/ShortUrl/search/{query}`
2. Kiểm tra xem `(query)` có rỗng hay không 
    - Nếu rỗng, trả về `BadRequest(”Query is required”);`
3. Chờ `_service.SearchAsync(query)` sau đó trả kết quả dựa trên `(query)` rồi gán vào `result`
    - Nếu trỗng, trả về `NotFound(”No matching URL found”);`
4. Trả về status `result` 

---

## 8. Filter URLs

### Endpoint

```
GET /api/ShortUrl/filter?page=1&pageSize=10&team=TMS&level=Error&createdDate=2025-07-01&sortBy=created&descending=true
```

### Response

```json
{
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalItems": 50,
  "totalPages": 5
}
```

### Luồng xử lý:

1. **[ShortUrlController]** nhận request tại `/api/ShortUrl/filter` 
2. API gọi `_service.FilterAsync(...)`, truyền vào tất cả các tham số lọc, phân trang và sắp xếp. Chờ kết quả gửi về gán vào:
    - `items`: danh sách các `ShortUrlVM` đã lọc
    - `totalCount`: tổng số kết quá (không phân trang)
3. Trả kết quả về client.

---

## 9. Update Short URL

### Endpoint

```
PUT /api/ShortUrl/{id}
```

### Request Body

```json
{
  "title": "New Title",
  "team": "TMS",
  "level": "Warning"
}
```

### Response(result)

```json
{
  "title": "New title2",
  "team": "OMS",
  "level": "Fatal"
}
```

### Luồng xử lý:

1. **[ShortUrlController]** nhận request tại `/api/ShortUrl/{id}`
2. Kiểm tra xem dữ liệu đầu vào `vm` có hợp lệ hay không 
    - Nếu không hợp lệ, trả về `BadRequest(ModelState);`
3. Gọi `_service.UpdateAsync()` chờ cập nhật thông tin với `id` tương ứng rồi gán vào `result`
    - Nếu `result` rỗng, trả về `NotFound("Host URL not found or deleted.");`
4. Trả về `result` 

---

## 10. Export to Excel

### Endpoint

```
GET /api/ShortUrl/export
```

### Response

Trả về file Excel `.xlsx` 

### Luồng xử lý:

1. **[ShortUrlController]** nhận request tại `/api/ShortUrl/export`
2. API gọi `_service.ExportToExcelAsync()` chờ trả về file Excel `.xlsx` chứa tất cả bản ghi trong DB

---

## 11. Import from Excel

### Endpoint

```
POST /api/ShortUrl/import
```

### Request: `multipart/form-data`

- Truyền vào file Excel

### Response

```json
{ "message": "14 records imported." }
```

### Luồng xử lý:

1. **[ShortUrlController]** nhận request tại `/api/ShortUrl/import` 
2. Kiểm tra file đầu vào có tồn tại hay rỗng không
    - Nếu file không hợp lệ, trả về `BadRequest(”No file uploaded”);`
3. Dùng `MemoryStream` để lưu trữ nội dung file trong bộ nhớ.
4. Sau khi ghi dữ liệu từ file vào stream, đặt lại con trỏ về đầu stream để chuẩn bị đọc nội dung.
5. Steam được truyền vào một service tên là `ImportFromExcelAsync` , service này sẽ:
    - Đọc dữ liệu từ file excel
    - Parse nội dung.
    - Thêm các bản ghi mới vào DB
6. Trả về `result` là số lượng bản ghi được import thành công
    - Nếu import thất bại, trả về `StatusCode(500, “Import failed.”);`

---

## 12. Redirect

### Endpoint:

```csharp
GET /api/r/{shortCode}
```

### Ví dụ:

```csharp
GET /api/r/abc12345
```

### Response:

- **302 Redirect** tới URL gốc.
- Nếu không tìm thấy → `404 Not Found`

### Luồng xử lý:

1. **[RedirectController]** nhận request với `/api/r{shortCode}`.
2. Gọi `_service.GetOriginalUrlAsync(shortCode)`
3. Trong **ShortUrlService**:
    - Dùng `_repo.GetByShortCodeAsync(shortCode)` để lấy entity từ DB.
    - Trả về `originalUrl` nếu tồn tại.
4. Nếu có URL → `return Redirect(originalUrl)` (HTTP 302).
5. Nếu không → trả lỗi 404 `"Shortened URL not found"`.

---

## 13. Statistics per Team & Level

### Endpoint

```
GET /api/stats/per-team?days=7
```

### Response

```json
{
  "TMS": {
    "Error": 12,
    "Info": 5
  },
  "Platform": {
    "Warning": 3
  }
}
```

### Luồng xử lý:

1. [StatisticController] nhận request với `/api/stats/per-team` 
2. Nếu `days` có giá trị → tính ngày bắt đầu `from = ngày hiện tại - days` (theo UTC)
3. Nếu `days` không có giá trị → `from = null` (không giới hạn thời gian)
4. API gọi `_service.GetTeamStats(from)` chờ lấy dữ liệu thống kê theo team từ ngày `from` trở đi (nếu có)
5. Trả về kết quả thống kê `result` 

- **Chi tiết Shortlink Repository**
    
    [Shortlink Repository](https://www.notion.so/Shortlink-Repository-23391f52357a800d8a65c2a5a80e2d80?pvs=21)
---
# Shortlink Repository

# 📘 ShortURL Repository Documentation

---

## 🔗 Tổng quan

Lớp `ShortUrlRepo` là repository chuyên biệt kế thừa từ `GenericRepo<ShortUrl>`, cung cấp các thao tác tùy chỉnh cho thực thể `ShortUrl`. Dưới đây là phần giải thích chi tiết theo **từng cụm dòng mã**:

# `ShortUrlRepo`

## Tóm tắt các chức năng chính

| Method | Chức năng chính |
| --- | --- |
| `GetByOriginalUrlAsync` | Kiểm tra URL gốc |
| `GetByShortCodeAsync` | Lấy URL từ mã rút gọn |
| `GetPaginatedAsync` | Phân trang danh sách |
| `GetFilteredAsync` | Lọc nâng cao |
| `GetStatsPerTeamAsync` | Thống kê theo nhóm |
| `SearchAsync` | Tìm kiếm nhanh |
| `ExistsByShortCodeAsync` | Kiểm tra trùng mã |
| `GenerateTitleFromUrl` | Sinh tiêu đề từ URL |
| `ExportToExcelAsync` | Xuất Excel |
| `ImportFromExcelAsync` | Nhập từ Excel |

---

## 1. Constructor

```csharp
public ShortUrlRepo(AppDbContext context, IOptions<ShortUrlSettings> options) : base(context)
```

- Gọi constructor lớp cha `GenericRepo` để kế thừa các thao tác cơ bản.
- Lưu `AppDbContext` vào `_context`.
- Lấy cấu hình tên miền (`BaseDomain`) để phục vụ sinh `ShortenedUrl`.

---

## 2. GetByOriginalUrlAsync

```csharp
public async Task<ShortUrl?> GetByOriginalUrlAsync(string originalUrl)
```

- Dùng EF để tìm bản ghi đầu tiên có `OriginalUrl` khớp với tham số.
- Trả về `null` nếu không tìm thấy.

---

## 3. GetByShortCodeAsync

```csharp
public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode)
```

- Kiểm tra các bản ghi `ShortenedUrl` có đuôi `"/" + shortCode` hay không.
- Đây là cách hệ thống truy xuất URL gốc từ mã rút gọn.

---

## 4. GetPaginatedAsync

```csharp
public async Task<(IList<ShortUrl>, int)> GetPaginatedAsync(int page, int pageSize)
```

- Lọc các bản ghi chưa bị xóa (`IsDeleted = false`).
- Sắp xếp theo `CreateDate`.
- Dùng `Skip()` và `Take()` để thực hiện phân trang.
- Trả về danh sách trang hiện tại và tổng số bản ghi.

---

## 5. GetFilteredAsync

```csharp
public async Task<(IList<ShortUrl>, int)> GetFilteredAsync(...)
```

- Bắt đầu với `AsQueryable()` để xây dựng query động.
- Lọc theo các điều kiện: `team`, `level`, `createdDate` nếu có.
- Sắp xếp theo `sortBy` và `descending`.
- Phân trang tương tự `GetPaginatedAsync`.

---

## 6. GetStatsPerTeamAsync

```csharp
public async Task<Dictionary<string, Dictionary<string, int>>> GetStatsPerTeamAsync(DateTime? from = null)
```

- Lọc theo `IsDeleted = false` và `CreateDate >= from` nếu có.
- Dùng `GroupBy` theo `Team` và `Level`.
- Sau đó gom tiếp thành dictionary: `Team -> (Level -> Count)`.

---

## 7. SearchAsync

```csharp
public async Task<ShortUrl?> SearchAsync(String query)
```

- Nếu `query` là URL hợp lệ → lấy `shortCode` từ phần cuối.
- Tìm `ShortUrl` trùng `OriginalUrl`, `ShortenedUrl` hoặc có đuôi `/shortCode`.

---

## 8. ExistsByShortCodeAsync

```csharp
public async Task<bool> ExistsByShortCodeAsync(string shortCode)
```

- Kiểm tra xem `ShortenedUrl` có kết thúc bằng `/r/{shortCode}` và chưa bị xóa không.

---

## 9. GenerateTitleFromUrl

```csharp
public async Task<string> GenerateTitleFromUrl(string url)
```

- Gửi HTTP request để tải nội dung HTML từ `url`.
- Trích xuất thẻ `<title>`.
- Nếu lỗi, lấy domain name từ `url` (bỏ "[www](http://www/).").

---

## 10. ExportToExcelAsync

```csharp
public async Task<MemoryStream> ExportToExcelAsync()
```

- Dùng EPPlus để tạo file Excel.
- Ghi tiêu đề cột ở dòng 1.
- Ghi từng dòng `ShortUrl` bắt đầu từ dòng 2.
- Xuất ra `MemoryStream` chứa file `.xlsx`.

---

## 11. ImportFromExcelAsync

```csharp
public async Task<int> ImportFromExcelAsync(Stream stream)
```

- Đọc sheet đầu tiên.
- Duyệt từng dòng từ dòng 2:
    - Nếu đã có `OriginalUrl` → cập nhật.
    - Nếu chưa có:
        - Nếu thiếu `ShortenedUrl`, tạo mới với `Guid`.
        - Gọi `GenerateTitleFromUrl` nếu thiếu `Title`.
        - Thêm bản ghi mới vào DB.
- Trả về số lượng bản ghi được nhập.
---
## Document chi tiết
- Shortlink API
  
  https://www.notion.so/Shortlink-API-23191f52357a80c9a63fcfb878e0d368?source=copy_link
- Shortlink Repository
  
  https://www.notion.so/Shortlink-Repository-23391f52357a800d8a65c2a5a80e2d80?source=copy_link
