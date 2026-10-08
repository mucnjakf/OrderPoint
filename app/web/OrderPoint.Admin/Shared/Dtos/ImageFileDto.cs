namespace OrderPoint.Admin.Shared.Dtos;

public sealed record ImageFileDto(byte[] Content, string ContentType, string FileName);