namespace testing.DTOs;

public class UpdateProfileRequest
{
    // Gunakan 'required' jika pakai .NET terbaru, atau hapus jika versi lama
    public string Username { get; set; } = string.Empty;

    // Kita tidak masukkan Password (karena ada endpoint khusus ganti password)
    // Kita tidak masukkan Role (karena user tidak boleh ganti role sendiri jadi admin)
}