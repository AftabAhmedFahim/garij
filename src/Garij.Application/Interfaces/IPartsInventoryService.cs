using Garij.Application.DTOs;

namespace Garij.Application.Interfaces;

public interface IPartsInventoryService
{
    Task<IEnumerable<PartDto>> GetAllPartsAsync();

    Task<PartDto?> GetPartByIdAsync(int id);

    Task<PartDto> AddPartAsync(PartDto part);

    Task<PartDto> UpdatePartAsync(PartDto part);

    Task DeletePartAsync(int id);

    Task AdjustStockAsync(int partId, int quantityDelta);

    Task<IEnumerable<PartDto>> GetLowStockPartsAsync();

    Task<JobPartUsedDto> RecordPartUsageAsync(JobPartUsedDto jobPartUsed);
}
