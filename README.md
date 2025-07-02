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

### Response (Success)

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

### Luồng xử lý nội bộ:

1. **[ShortUrlController]** nhận request từ client tại `/api/ShortUrl`.
2. Gọi `_service.ShortenUrlAsync(vm)` trong **`ShortUrlService`**.
3. **ShortUrlService**:
    - Kiểm tra xem `originalUrl` đã tồn tại trong DB chưa:
        - ✅ Nếu có → trả lại short link cũ.
        - ❌ Nếu chưa → tạo mới:
            - Generate short code: `Guid.NewGuid().ToString("N").Substring(0, 8)`
            - Gán `shortenedUrl = BaseDomain + /r/ + shortCode`
            - Lưu vào DB thông qua `_repo.CreateAsync(entity)`
4. Commit DB với `SaveChangesAsync()`.
5. Trả về `ShortUrlVM` đã được map từ Entity.

---

## 2. API Redirect từ Short URL

### Endpoint:

```csharp
GET /api/r/{shortCode
```

### Ví dụ:

```csharp
GET /api/r/abc12345
```

### Response:

- **302 Redirect** tới URL gốc.
- Nếu không tìm thấy → `404 Not Found`

### Luồng xử lý nội bộ:

1. **[RedirectController]** nhận request với `{shortCode}`.
2. Gọi `_service.GetOriginalUrlAsync(shortCode)`
3. Trong **ShortUrlService**:
    - Dùng `_repo.GetByShortCodeAsync(shortCode)` để lấy entity từ DB.
    - Trả về `originalUrl` nếu tồn tại.
4. Nếu có URL → `return Redirect(originalUrl)` (HTTP 302).
5. Nếu không → trả lỗi 404 `"Shortened URL not found"`.

---

## Một số lưu ý kỹ thuật

- Base API URL:  http://alm-api/api
- `shortCode` không lưu riêng mà là một phần trong `ShortenedUrl` (`/r/{shortCode}`).
- `BaseDomain` được cấu hình thông qua `IOptions<ShortUrlSettings>`.
- Shortened URL là duy nhất cho mỗi `originalUrl`, nên sẽ không sinh trùng lặp.
