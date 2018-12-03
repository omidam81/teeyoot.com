using System.Linq;
using Orchard.Data;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.Module.Services
{
    public class DeliverySettingsService : IDeliverySettingsService
    {
        private readonly IRepository<DeliverySettingRecord> _deliverySettingsRepository;
        private readonly IRepository<CountryRecord> _countryRepository;

        public DeliverySettingsService(
            IRepository<DeliverySettingRecord> deliverySettingsRepository,
            IRepository<CountryRecord> countryRepository)
        {
            _deliverySettingsRepository = deliverySettingsRepository;
            _countryRepository = countryRepository;
        }

        public void DeleteSetting(int id)
        {
            _deliverySettingsRepository.Delete(_deliverySettingsRepository.Get(id));
        }

        public void UpdateSetting(DeliverySettingRecord setting)
        {
            _deliverySettingsRepository.Update(setting);
        }

        public void AddSetting(
            string state,
            double postageCost,
            double codCost,
            int countryId, int timetodeliver
            )
        {
            var newRecord = new DeliverySettingRecord()
            {
                State = state,
                Country = _countryRepository.Get(countryId),
                PostageCost = postageCost,
                CodCost = codCost,
                DeliveryTime = timetodeliver
            };

            _deliverySettingsRepository.Create(newRecord);
        }

        public DeliverySettingRecord GetSettingById(int settingId)
        {
            return _deliverySettingsRepository.Get(f => f.Id == settingId);
        }

        public IQueryable<DeliverySettingRecord> GetAllSettings(int countryId)
        {
            var country = _countryRepository.Get(countryId);
            var settings = _deliverySettingsRepository.Table
                .Where(s => s.Country == country);

            return settings;
        }

        public void EditSetting(EditDeliverySettingViewModel viewModel)
        {
            var record = _deliverySettingsRepository.Get(f => f.Id == viewModel.Id);
            record.State = viewModel.State;
            record.Country = _countryRepository.Get(viewModel.CountryId);
            record.PostageCost = viewModel.PostageCost;
            record.CodCost = viewModel.CodCost;
            record.DeliveryTime = viewModel.DeliveryTime;
            _deliverySettingsRepository.Update(record);
        }
    }
}