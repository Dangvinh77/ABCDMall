# Movies Image Upload Checklist

## 1. Muc tieu

Bo sung kha nang upload anh that cho module `movies admin`, thay vi chi nhap `PosterUrl` dang text nhu hien tai.

Can phan biet ro:

- `thumbnail/poster`: anh dai dien phim
- `backdrop/banner`: anh ngang dung cho hero/banner
- co the giu `trailerUrl` dang text URL

## 2. Hien trang source

- Frontend admin form phim hien chi co input text `Poster URL`
- Backend `MoviesAdminMovieUpsertDto` chi co `PosterUrl`
- Entity `Movie` hien chi co `PosterUrl`, chua co field rieng cho `ThumbnailUrl` va `BackdropUrl`
- API tao/sua phim hien nhan JSON `[FromBody]`, chua nhan `multipart/form-data`

## 3. Checklist backend

### A. Chot mo hinh du lieu anh

- [ ] Quyet dinh co giu `PosterUrl` lam thumbnail chinh hay tach thanh:
  - `ThumbnailUrl`
  - `BackdropUrl`
  - `PosterUrl` (neu can giu vi compatibility)
- [ ] Chot ten field va quy uoc su dung tren frontend/public pages

### B. Cap nhat entity va schema

- [ ] Cap nhat entity `Movie`
- [ ] Them field DB can thiet:
  - `ThumbnailUrl`
  - `BackdropUrl`
- [ ] Tao migration moi cho movies catalog schema
- [ ] Dam bao seed/movie data cu van chay duoc

### C. Them service luu file

- [ ] Tao service upload file cho Movies, khong dung tam service cua Users neu logic khac biet
- [ ] Chot noi luu file:
  - local folder trong backend
  - hoac cloud/object storage ve sau
- [ ] Chot folder convention:
  - `/uploads/movies/thumbnails`
  - `/uploads/movies/backdrops`
- [ ] Tao rule dat ten file tranh trung

### D. Them validation upload

- [ ] Gioi han dinh dang file:
  - jpg
  - jpeg
  - png
  - webp
- [ ] Gioi han dung luong file
- [ ] Chan file khong phai image
- [ ] Can nhac resize/optimize anh neu can

### E. Cap nhat DTO va endpoints

- [ ] Tao request model moi cho create/update movie bang `multipart/form-data`
- [ ] Ho tro field text + file trong cung request
- [ ] Ho tro 2 cach:
  - upload file moi
  - giu URL cu neu khong thay doi anh
- [ ] Cap nhat `MoviesAdminController` cho create/update movie

### F. Phuc vu file tĩnh

- [ ] Dam bao backend expose static files cho thu muc uploads
- [ ] Chot URL public cho image sau khi upload
- [ ] Kiem tra frontend co render duoc URL anh tu backend

### G. Xu ly cap nhat va xoa file cu

- [ ] Neu admin upload anh moi, quyet dinh co xoa file cu hay khong
- [ ] Neu movie bi disable/delete, quyet dinh co giu file hay xoa file
- [ ] Tranh de rac file tang dan trong local storage

## 4. Checklist frontend

### A. Cap nhat form movie admin

- [ ] Them input file cho:
  - thumbnail/poster
  - backdrop/banner
- [ ] Van giu cac field text khac
- [ ] Doi request tu JSON sang `FormData`

### B. Preview anh

- [ ] Preview thumbnail da chon truoc khi submit
- [ ] Preview backdrop da chon truoc khi submit
- [ ] Hien anh hien tai khi edit movie
- [ ] Co nut remove/reset image

### C. UX va validation

- [ ] Bao loi ro neu file sai format
- [ ] Bao loi ro neu file qua lon
- [ ] Hien loading khi upload
- [ ] Khong cho submit lap khi dang upload

### D. List/detail hien thi anh

- [ ] Hien cot thumbnail trong bang movies admin neu can
- [ ] Dam bao public movies pages dung dung field anh moi
- [ ] Fallback image neu phim chua co anh

## 5. Checklist compatibility

- [ ] Khong lam vo movies cu dang dung `PosterUrl`
- [ ] Co mapping/fallback:
  - neu chua co `ThumbnailUrl` thi dung `PosterUrl`
- [ ] Kiem tra seed data cu van hien anh binh thuong

## 6. Checklist test

### Backend

- [ ] Test upload file hop le
- [ ] Test reject file sai dinh dang
- [ ] Test reject file qua size
- [ ] Test create movie co anh
- [ ] Test update movie thay anh
- [ ] Test static file URL truy cap duoc

### Frontend

- [ ] Test create movie voi thumbnail + backdrop
- [ ] Test edit movie va preview
- [ ] Test movie list render anh moi
- [ ] Test fallback neu movie khong co anh

## 7. Thu tu trien khai de xuat

- [ ] Buoc 1: chot model field anh (`ThumbnailUrl`, `BackdropUrl`)
- [ ] Buoc 2: migration + entity + repository
- [ ] Buoc 3: file storage service + static files
- [ ] Buoc 4: admin create/update API bang `multipart/form-data`
- [ ] Buoc 5: frontend form upload + preview
- [ ] Buoc 6: test end-to-end

## 8. Khuyen nghi

- Nen lam toi thieu 2 field rieng:
  - `ThumbnailUrl`
  - `BackdropUrl`
- Khong nen tiep tuc chi dung mot `PosterUrl` cho moi vi tri hien thi
- Neu can nhanh, co the:
  - giu `PosterUrl` cho compatibility
  - them moi `BackdropUrl`
  - sau do doi ten/refactor o dot sau
