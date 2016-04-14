﻿using System;
using BodyReportMobile.Core.ViewModels;
using Message;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using BodyReportMobile.Core.ServiceManagers;
using SQLite.Net;
using BodyReportMobile.Core.Message.Binding;
using Framework;
using XLabs.Ioc;
using BodyReportMobile.Core.Framework;
using BodyReportMobile.Core.Framework.Binding;
using BodyReportMobile.Core.WebServices;
using Xamarin.Forms;

namespace BodyReportMobile.Core.ViewModels
{
	public class TrainingJournalViewModel : BaseViewModel
	{
		List<TrainingWeek> _trainingWeekList = null;
		public ObservableCollection<GenericGroupModelCollection<BindingTrainingWeek>> GroupedTrainingWeeks { get; set; } = new ObservableCollection<GenericGroupModelCollection<BindingTrainingWeek>>();

		private SQLiteConnection _dbContext;
		private TrainingWeekManager _trainingWeekManager;
		private bool isBusy;

		private string _createLabel = string.Empty;

		public TrainingJournalViewModel () : base()
        {
			_dbContext = Resolver.Resolve<ISQLite> ().GetConnection ();
			_trainingWeekManager = new TrainingWeekManager (_dbContext);
		}

		protected async override void Show()
		{
			base.Show();

			_trainingWeekList = _trainingWeekManager.FindTrainingWeek (null, false);
			SynchronizeData ();

			await RetreiveAndSaveOnlineData ();
		}

		protected override void InitTranslation()
		{
			base.InitTranslation ();

			TitleLabel = Translation.Get (TRS.TRAINING_JOURNAL);
			CreateLabel = Translation.Get (TRS.CREATE);
		}

		private async Task RetreiveAndSaveOnlineData ()
		{
			try
			{
				if (IsBusy)
					return;
				IsBusy = true;

				var onlineTrainingWeekList = await TrainingWeekService.FindTrainingWeeks ();
				if (onlineTrainingWeekList != null)
				{
					var list = _trainingWeekManager.FindTrainingWeek (null, true);
					if (list != null)
					{
						foreach (var trainingWeek in list)
							_trainingWeekManager.DeleteTrainingWeek (trainingWeek);
					}

					_trainingWeekList = new List<TrainingWeek> ();
					foreach (var trainingWeek in onlineTrainingWeekList)
                        _trainingWeekList.Add (_trainingWeekManager.UpdateTrainingWeek (trainingWeek));
					SynchronizeData ();
				}
                IsBusy = false;
            }
			catch (Exception except)
			{
                IsBusy = false;
            }
		}

		public void SynchronizeData ()
		{
			//Create BindingCollection
			int currentYear = 0;
			GroupedTrainingWeeks.Clear ();

			if (_trainingWeekList != null)
			{
				_trainingWeekList = _trainingWeekList.OrderByDescending (m => m.Year).ThenByDescending (m => m.WeekOfYear).ToList ();

				DateTime dateTime;
				var localGroupedTrainingWeeks = new ObservableCollection<GenericGroupModelCollection<BindingTrainingWeek>> ();
				GenericGroupModelCollection<BindingTrainingWeek> collection = null;
				foreach (var trainingWeek in _trainingWeekList)
				{
					if (collection == null || currentYear != trainingWeek.Year)
					{
						currentYear = trainingWeek.Year;
						collection = new GenericGroupModelCollection<BindingTrainingWeek> ();
						collection.LongName = currentYear.ToString ();
						collection.ShortName = currentYear.ToString ();
						localGroupedTrainingWeeks.Add (collection);
					}

					dateTime = Utils.YearWeekToPlanningDateTime(trainingWeek.Year, trainingWeek.WeekOfYear);
					collection.Add (new BindingTrainingWeek () {
						Date = string.Format(Translation.Get(TRS.FROM_THE_P0TH_TO_THE_P1TH_OF_P2_P3), dateTime.Day, dateTime.AddDays(6.0d).Day, Translation.Get(((TMonthType)dateTime.Month).ToString().ToUpper()), dateTime.Year),
						Week = Translation.Get(TRS.WEEK_NUMBER) + ' ' + trainingWeek.WeekOfYear.ToString (),
						TrainingWeek = trainingWeek
					});
				}

				foreach (var trainingWeek in localGroupedTrainingWeeks)
				{
					GroupedTrainingWeeks.Add (trainingWeek);
				}
			}
		}

		public ICommand RefreshDataCommand
		{
			get
			{
				return new Command(async () => { await RetreiveAndSaveOnlineData(); });
			}
		}

		public ICommand CreateNewCommand
		{
			get
			{
				return new Command (async () => { await CreateNewTrainingWeek(); });
			}
		}

		private async Task CreateNewTrainingWeek ()
		{
			var trainingWeek = new TrainingWeek () {
				Year = 2016,
				WeekOfYear = 9,
				UserHeight = 193,
				UserWeight = 90
			};

			if (await EditTrainingWeekViewModel.Show (trainingWeek, TEditMode.Create, this))
			{
				_trainingWeekList.Add (trainingWeek);
				SynchronizeData ();
			}
		}

		public ICommand CopyCommand
		{
			get
			{
				return new Command (() =>
				{
                    
				});
			}
		}

		public ICommand DeleteCommand
		{
			get
			{
				return new Command (() =>
				{
				});
			}
		}

		#region accessor

		public string CreateLabel {
			get {
				return _createLabel;
			}
			set {
				_createLabel = value;
				OnPropertyChanged ();
			}
		}

		public bool IsBusy
		{
			get { return isBusy; }
			set
			{
				if (isBusy == value)
					return;

				isBusy = value;
                OnPropertyChanged();
            }
		}

		#endregion
	}
}

