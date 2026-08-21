using Garij.Application.DTOs;
using Garij.Application.Interfaces;

namespace Garij.Application.Services;

public class PartsInventoryService : IPartsInventoryService
{
    public Task<IEnumerable<PartDto>> GetAllPartsAsync() => throw new NotImplementedException();

    public Task<PartDto?> GetPartByIdAsync(int id) => throw new NotImplementedException();

    public Task<PartDto> AddPartAsync(PartDto part) => throw new NotImplementedException();

    public Task<PartDto> UpdatePartAsync(PartDto part) => throw new NotImplementedException();

    public Task DeletePartAsync(int id) => throw new NotImplementedException();

    public Task AdjustStockAsync(int partId, int quantityDelta) => throw new NotImplementedException();

    public Task<IEnumerable<PartDto>> GetLowStockPartsAsync() => throw new NotImplementedException();

    public Task<JobPartUsedDto> RecordPartUsageAsync(JobPartUsedDto jobPartUsed) => throw new NotImplementedException();
}
