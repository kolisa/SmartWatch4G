using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Infrastructure.Services;

public class IwownCalculationService
{
    private readonly HttpClient _http;
    private readonly ILogger<IwownCalculationService> _logger;

    public IwownCalculationService(HttpClient http, ILogger<IwownCalculationService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SleepCalculationResponse?> CalculateSleepAsync(SleepCalculationRequest req)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/calculation/sleep", req);
            return await response.Content.ReadFromJsonAsync<SleepCalculationResponse>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error calling /calculation/sleep"); return null; }
    }

    public async Task<EcgCalculationResponse?> CalculateEcgAsync(EcgCalculationRequest req)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/calculation/ecg", req);
            return await response.Content.ReadFromJsonAsync<EcgCalculationResponse>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error calling /calculation/ecg"); return null; }
    }

    public async Task<AfCalculationResponse?> CalculateAfAsync(AfCalculationRequest req)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/calculation/af", req);
            return await response.Content.ReadFromJsonAsync<AfCalculationResponse>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error calling /calculation/af"); return null; }
    }

    public async Task<Spo2CalculationResponse?> CalculateSpo2Async(Spo2CalculationRequest req)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/calculation/spo2", req);
            return await response.Content.ReadFromJsonAsync<Spo2CalculationResponse>();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error calling /calculation/spo2"); return null; }
    }
}
