﻿using Acr.UserDialogs;
using BodyReport.Message;
using BodyReportMobile.Core.Crud.Transformer;
using BodyReportMobile.Core.Data;
using BodyReportMobile.Core.Framework;
using BodyReportMobile.Core.Message.Binding;
using BodyReportMobile.Core.ServiceLayers;
using BodyReportMobile.Core.WebServices;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using XLabs.Ioc;

namespace BodyReportMobile.Core.ViewModels
{
    public class EditTrainingExerciseViewModelResult
    {
        public bool Result = false;
    }

    public class EditTrainingExerciseViewModel : BaseViewModel
    {
        private SQLiteConnection _dbContext;
        private UserInfo _userInfo;
        private TrainingExercise _trainingExercise;
        TrainingDayService _trainingDayService;
        private IUserDialogs _userDialog;

        public EditTrainingExerciseViewModel() : base()
        {
            _dbContext = Resolver.Resolve<ISQLite>().GetConnection();
            _trainingDayService = new TrainingDayService(_dbContext);
            _userDialog = Resolver.Resolve<IUserDialogs>();
        }

        public static async Task<EditTrainingExerciseViewModelResult> ShowAsync(TrainingExercise trainingExercise, BaseViewModel parent = null)
        {
            
            var viewModel = new EditTrainingExerciseViewModel();
            viewModel._trainingExercise = trainingExercise;
            var result = await ShowModalViewModelAsync(viewModel, parent);

            var editTrainingExerciseViewModelResult = new EditTrainingExerciseViewModelResult();
            editTrainingExerciseViewModelResult.Result = result;
            return editTrainingExerciseViewModelResult;
        }

        protected override async Task ShowAsync()
        {
            await base.ShowAsync();

            try
            {
                ActionIsInProgress = true;
                await SynchronizeDataAsync();
            }
            catch
            {}
            finally
            {
                ActionIsInProgress = false;
            }
        }

        protected override void InitTranslation()
        {
            base.InitTranslation();
            TitleLabel = Translation.Get(TRS.TRAINING_EXERCISE);
            ExerciseTitleLabel = Translation.Get(TRS.NAME);
            RestTimeLabel = Translation.Get(TRS.REST_TIME) + " (sec)";
            EccentricContractionTempoLabel = Translation.Get(TRS.ECCENTRIC_CONTRACTION);
            StretchPositionTempoLabel = Translation.Get(TRS.STRETCH_POSITION);
            ConcentricContractionTempoLabel = Translation.Get(TRS.CONCENTRIC_CONTRACTION);
            ContractedPositionTempoLabel = Translation.Get(TRS.CONTRACTED_POSITION);
            AddRepsLabel = Translation.Get(TRS.ADD_REPS);
            ValidateLabel = Translation.Get(TRS.VALIDATE);
        }

        private async Task<TUnitType> GetExerciseUnit()
        {
            TUnitType unit = TUnitType.Metric;
            try
            {
                if (_trainingExercise != null && _trainingExercise.TrainingExerciseSets != null &&
                    _trainingExercise.TrainingExerciseSets.Count > 0)
                {
                    unit = _trainingExercise.TrainingExerciseSets[0].Unit;
                }
                else
                {
                    if (_userInfo == null)
                    {
                        var userInfoKey = new UserInfoKey() { UserId = _trainingExercise.UserId };
                        if (_trainingExercise.UserId == UserData.Instance.UserInfo.UserId)
                        {
                            var userInfoService = new UserInfoService(_dbContext);
                            _userInfo = userInfoService.GetUserInfo(userInfoKey);
                        }
                        else
                            _userInfo = await UserInfoWebService.GetUserInfoAsync(userInfoKey);
                    }
                    if (_userInfo != null)
                        unit = _userInfo.Unit;
                }
            }
            catch
            {
            }
            return unit;
        }

        private async Task SynchronizeDataAsync()
        {
            var unit = await GetExerciseUnit();
            string weightUnit = "kg";
            if (unit == TUnitType.Imperial)
            {
                weightUnit = Translation.Get(TRS.POUND);
            }

            ExerciseTitle = Translation.GetInDB(BodyExerciseTransformer.GetTranslationKey(_trainingExercise.BodyExerciseId));
            RestTime = _trainingExercise.RestTime;
            EccentricContractionTempo = _trainingExercise.EccentricContractionTempo;
            StretchPositionTempo = _trainingExercise.StretchPositionTempo;
            ConcentricContractionTempo = _trainingExercise.ConcentricContractionTempo;
            ContractedPositionTempo = _trainingExercise.ContractedPositionTempo;

            string urlImages = HttpConnector.Instance.BaseUrl + "images/bodyexercises/{0}";
            var imageName = string.Format("{0}.png", _trainingExercise.BodyExerciseId);
            var urlImage = string.Format(urlImages, imageName);
            var localImagePath = Path.Combine(AppTools.BodyExercisesImagesDirectory, imageName);
            await AppTools.Instance.CachingImageAsync(_trainingExercise, urlImage, localImagePath, null);
            ExerciseImageSource = localImagePath;

            if (BindingTrainingExerciseSetReps == null)
                BindingTrainingExerciseSetReps = new ObservableCollection<BindingTrainingExerciseSetRep>();
            else
                BindingTrainingExerciseSetReps.Clear();

            if (_trainingExercise != null)
            {
                BindingTrainingExerciseSetRep bindingSetRep;
                if (_trainingExercise.TrainingExerciseSets == null || _trainingExercise.TrainingExerciseSets.Count == 0)
                {
                    bindingSetRep = new BindingTrainingExerciseSetRep();
                    bindingSetRep.RepsLabel = Translation.Get(TRS.REPS);
                    bindingSetRep.WeightsLabel = Translation.Get(TRS.WEIGHT) + "(" + weightUnit + ")";
                    bindingSetRep.Reps = 8;
                    bindingSetRep.Weights = 0;
                    BindingTrainingExerciseSetReps.Add(bindingSetRep);
                }
                else
                {
                    int count = 0;
                    foreach (var trainingExerciseSet in _trainingExercise.TrainingExerciseSets)
                    {
                        for (int i = 0; i < trainingExerciseSet.NumberOfSets; i++)
                        {
                            bindingSetRep = new BindingTrainingExerciseSetRep();
                            if (count == 0)
                            {
                                bindingSetRep.RepsLabel = Translation.Get(TRS.REPS);
                                bindingSetRep.WeightsLabel = Translation.Get(TRS.WEIGHT) + "(" + weightUnit + ")";
                            }
                            else
                                bindingSetRep.RepsLabel = bindingSetRep.WeightsLabel = string.Empty; // necessary for trigger Text.Length
                            bindingSetRep.Reps = trainingExerciseSet.NumberOfReps;
                            bindingSetRep.Weights = trainingExerciseSet.Weight;
                            BindingTrainingExerciseSetReps.Add(bindingSetRep);
                            count++;
                        }
                    }
                }
                if (BindingTrainingExerciseSetReps.Count > 0)
                    BindingTrainingExerciseSetReps[BindingTrainingExerciseSetReps.Count - 1].BtnPlusVisible = true;
            }
        }

        private void AddRepAction()
        {
            try
            {
                int previousRep = 0;
                double previousWeight = 0;
                if (BindingTrainingExerciseSetReps.Count > 0)
                {
                    previousRep = BindingTrainingExerciseSetReps[BindingTrainingExerciseSetReps.Count-1].Reps;
                    previousWeight = BindingTrainingExerciseSetReps[BindingTrainingExerciseSetReps.Count-1].Weights;
                }

                foreach (var bindingSetRep in BindingTrainingExerciseSetReps)
                {
                    if (bindingSetRep.BtnPlusVisible)
                        bindingSetRep.BtnPlusVisible = false;
                }

                BindingTrainingExerciseSetReps.Add(new BindingTrainingExerciseSetRep()
                {
                    Reps = previousRep,
                    Weights = previousWeight,
                    BtnPlusVisible = true,
                    RepsLabel = string.Empty,// necessary for trigger Text.Length
                    WeightsLabel = string.Empty // necessary for trigger Text.Length
                });

                
            }
            catch (Exception except)
            {
                ILogger.Instance.Error("Unable to add rep in training exercise", except);
            }
        }

        private void DeleteRepAction(BindingTrainingExerciseSetRep bindingTrainingExerciseSetRep)
        {
            if (bindingTrainingExerciseSetRep == null)
                return;
            try
            {
                if (BindingTrainingExerciseSetReps.Count > 0 && BindingTrainingExerciseSetReps.IndexOf(bindingTrainingExerciseSetRep) != -1)
                {
                    BindingTrainingExerciseSetReps.Remove(bindingTrainingExerciseSetRep);
                    if (BindingTrainingExerciseSetReps.Count > 0)
                    {
                        BindingTrainingExerciseSetReps[BindingTrainingExerciseSetReps.Count-1].BtnPlusVisible = true;
                    }
                }
            }
            catch (Exception except)
            {
                ILogger.Instance.Error("Unable to supress rep in training exercise", except);
            }
        }

        private async Task ValidateActionAsync()
        {
            try
            {
                if(BindingTrainingExerciseSetReps != null && BindingTrainingExerciseSetReps.Count > 0 &&
                   _trainingExercise != null)
                {
                    var trainingExercise = _trainingExercise.Clone();
                    if (trainingExercise.TrainingExerciseSets == null)
                        trainingExercise.TrainingExerciseSets = new List<TrainingExerciseSet>();
                    else
                        trainingExercise.TrainingExerciseSets.Clear(); // empty sets for replacing

                    trainingExercise.RestTime = RestTime; // don't forget restime...
                    trainingExercise.EccentricContractionTempo = EccentricContractionTempo;
                    trainingExercise.StretchPositionTempo = StretchPositionTempo;
                    trainingExercise.ConcentricContractionTempo = ConcentricContractionTempo;
                    trainingExercise.ContractedPositionTempo = ContractedPositionTempo;
                    int nbSet = 0, currentRepValue = 0;
                    var tupleSetRepList = new List<Tuple<int, int, double>>();
                    int repValue;
                    double weightValue, currentWeightValue = 0;
                    for (int i = 0; i < BindingTrainingExerciseSetReps.Count; i++)
                    {
                        repValue = BindingTrainingExerciseSetReps[i].Reps;
                        weightValue = BindingTrainingExerciseSetReps[i].Weights;
                        if (repValue == 0)
                            continue;

                        if (weightValue == currentWeightValue && repValue == currentRepValue)
                            nbSet++;
                        else
                        {
                            if (nbSet != 0)
                                tupleSetRepList.Add(new Tuple<int, int, double>(nbSet, currentRepValue, currentWeightValue));
                            currentRepValue = repValue;
                            currentWeightValue = weightValue;
                            nbSet = 1;
                        }
                    }

                    //last data
                    if (nbSet != 0)
                        tupleSetRepList.Add(new Tuple<int, int, double>(nbSet, currentRepValue, currentWeightValue));

                    var unit = await GetExerciseUnit();
                    int id = 1;
                    foreach (Tuple<int, int, double> tupleSetRep in tupleSetRepList)
                    {
                        trainingExercise.TrainingExerciseSets.Add(new TrainingExerciseSet()
                        {
                            UserId = trainingExercise.UserId,
                            Year = trainingExercise.Year,
                            WeekOfYear = trainingExercise.WeekOfYear,
                            DayOfWeek = trainingExercise.DayOfWeek,
                            TrainingDayId = trainingExercise.TrainingDayId,
                            TrainingExerciseId = trainingExercise.Id,
                            Id = id,
                            NumberOfSets = tupleSetRep.Item1,
                            NumberOfReps = tupleSetRep.Item2,
                            Weight = tupleSetRep.Item3,
                            Unit = unit
                        });
                        id++;
                    }

                    //Save in server
                    var trainingDayKey = new TrainingDayKey()
                    {
                        UserId = trainingExercise.UserId,
                        Year = trainingExercise.Year,
                        WeekOfYear = trainingExercise.WeekOfYear,
                        DayOfWeek = trainingExercise.DayOfWeek,
                        TrainingDayId = trainingExercise.TrainingDayId
                    };
                    var trainingDayScenario = new TrainingDayScenario() { ManageExercise = true };
                    var trainingDay = await TrainingDayWebService.GetTrainingDayAsync(trainingDayKey, trainingDayScenario);

                    //modify datas
                    var trainingExerciseTmp = trainingDay.TrainingExercises.Where(t => TrainingExerciseKey.IsEqualByKey(t, trainingExercise)).FirstOrDefault();
                    var indexOf = trainingDay.TrainingExercises.IndexOf(trainingExerciseTmp);
                    if (indexOf != -1)
                    {
                        //Replace exercise and sets
                        trainingDay.TrainingExercises[indexOf] = trainingExercise;
                        //update to server
                        trainingDay = await TrainingDayWebService.UpdateTrainingDayAsync(trainingDay, trainingDayScenario);
                        //Save modified data in local database
                        _trainingDayService.UpdateTrainingDay(trainingDay, trainingDayScenario);

                        CloseViewModel();
                    }
                }
            }
            catch (Exception except)
            {
                ILogger.Instance.Error("Unable to validate training rep/set", except);
                await _userDialog.AlertAsync(except.Message, Translation.Get(TRS.ERROR), Translation.Get(TRS.OK));
            }
        }

        #region Binding Properties

        private ObservableCollection<BindingTrainingExerciseSetRep> _bindingTrainingExerciseSetReps;
        public ObservableCollection<BindingTrainingExerciseSetRep> BindingTrainingExerciseSetReps
        {
            get { return _bindingTrainingExerciseSetReps; }
            set
            {
                _bindingTrainingExerciseSetReps = value;
                OnPropertyChanged();
            }
        }
        
        private string _exerciseTitleLabel;
        public string ExerciseTitleLabel
        {
            get { return _exerciseTitleLabel; }
            set
            {
                _exerciseTitleLabel = value;
                OnPropertyChanged();
            }
        }

        private string _exerciseTitle;
        public string ExerciseTitle
        {
            get { return _exerciseTitle; }
            set
            {
                _exerciseTitle = value;
                OnPropertyChanged();
            }
        }

        private string _restTimeLabel;
        public string RestTimeLabel
        {
            get { return _restTimeLabel; }
            set
            {
                _restTimeLabel = value;
                OnPropertyChanged();
            }
        }

        private int _restTime;
        public int RestTime
        {
            get { return _restTime; }
            set
            {
                _restTime = value;
                OnPropertyChanged();
            }
        }

        private string _eccentricContractionTempoLabel;
        public string EccentricContractionTempoLabel
        {
            get { return _eccentricContractionTempoLabel; }
            set
            {
                _eccentricContractionTempoLabel = value;
                OnPropertyChanged();
            }
        }

        private int _eccentricContractionTempo;
        public int EccentricContractionTempo
        {
            get { return _eccentricContractionTempo; }
            set
            {
                _eccentricContractionTempo = value;
                OnPropertyChanged();
            }
        }

        private string _stretchPositionTempoLabel;
        public string StretchPositionTempoLabel
        {
            get { return _stretchPositionTempoLabel; }
            set
            {
                _stretchPositionTempoLabel = value;
                OnPropertyChanged();
            }
        }

        private int _stretchPositionTempo;
        public int StretchPositionTempo
        {
            get { return _stretchPositionTempo; }
            set
            {
                _stretchPositionTempo = value;
                OnPropertyChanged();
            }
        }

        private string _concentricContractionTempoLabel;
        public string ConcentricContractionTempoLabel
        {
            get { return _concentricContractionTempoLabel; }
            set
            {
                _concentricContractionTempoLabel = value;
                OnPropertyChanged();
            }
        }

        private int _concentricContractionTempo;
        public int ConcentricContractionTempo
        {
            get { return _concentricContractionTempo; }
            set
            {
                _concentricContractionTempo = value;
                OnPropertyChanged();
            }
        }

        private string _contractedPositionTempoLabel;
        public string ContractedPositionTempoLabel
        {
            get { return _contractedPositionTempoLabel; }
            set
            {
                _contractedPositionTempoLabel = value;
                OnPropertyChanged();
            }
        }

        private int _contractedPositionTempo;
        public int ContractedPositionTempo
        {
            get { return _contractedPositionTempo; }
            set
            {
                _contractedPositionTempo = value;
                OnPropertyChanged();
            }
        }

        private string _exerciseImageSource;
        public string ExerciseImageSource
        {
            get { return _exerciseImageSource; }
            set
            {
                _exerciseImageSource = value;
                OnPropertyChanged();
            }
        }

        private string _addRepsLabel;
        public string AddRepsLabel
        {
            get { return _addRepsLabel; }
            set
            {
                _addRepsLabel = value;
                OnPropertyChanged();
            }
        }

        private string _validateLabel;
        public string ValidateLabel
        {
            get { return _validateLabel; }
            set
            {
                _validateLabel = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Command

        private ICommand _addRepCommand = null;
        public ICommand AddRepCommand
        {
            get
            {
                if (_addRepCommand == null)
                {
                    _addRepCommand = new ViewModelCommand(this, () =>
                    {
                        AddRepAction();
                    });
                }
                return _addRepCommand;
            }
        }

        private ICommand _deleteRepCommand = null;
        public ICommand DeleteRepCommand
        {
            get
            {
                if (_deleteRepCommand == null)
                {
                    _deleteRepCommand = new ViewModelCommand(this, (bindingTrainingExerciseSetRep) =>
                    {
                        DeleteRepAction(bindingTrainingExerciseSetRep as BindingTrainingExerciseSetRep);
                    });
                }
                return _deleteRepCommand;
            }
        }

        private ICommand _validateCommand = null;
        public ICommand ValidateCommand
        {
            get
            {
                if (_validateCommand == null)
                {
                    _validateCommand = new ViewModelCommandAsync(this, async () =>
                    {
                        await ValidateActionAsync();
                    });
                }
                return _validateCommand;
            }
        }

        #endregion
    }
}
