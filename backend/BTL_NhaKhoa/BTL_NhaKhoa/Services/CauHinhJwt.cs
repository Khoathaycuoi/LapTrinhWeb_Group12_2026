using Microsoft.IdentityModel.Tokens;

namespace BTL_NhaKhoa.Services;

public sealed record CauHinhJwt(SecurityKey KhoaKy, string NhaPhatHanh, string DoiTuongNhan);
