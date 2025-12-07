namespace testing.DTOs;

public class RuanganCreateRequest
{
    public required string Nama { get; set; }
}

public class RuanganUpdateRequest
{
    public required string Nama { get; set; }
}

public class RuanganDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
}