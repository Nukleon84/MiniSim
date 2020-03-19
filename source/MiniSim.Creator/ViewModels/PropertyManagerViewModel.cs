using Caliburn.Micro;
using MiniSim.Core.Expressions;
using MiniSim.Core.PropertyDatabase;
using MiniSim.Core.Thermodynamics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Creator.ViewModels
{
    public class PropertyManagerViewModel : Screen
    {
        IEventAggregator _eventAggregator;

        ThermodynamicSystem _currentPropertyBlock;
        IList<ThermodynamicSystem> _availablePropertyBlocks;
        public IList<ThermodynamicSystem> AvailablePropertyBlocks { get => _availablePropertyBlocks; set { _availablePropertyBlocks = value; NotifyOfPropertyChange(() => AvailablePropertyBlocks); } }
        public ThermodynamicSystem CurrentPropertyBlock { get => _currentPropertyBlock; set { _currentPropertyBlock = value; NotifyOfPropertyChange(() => CurrentPropertyBlock); ChangePropertyBlock(); } }

        public string SubstanceFilter { get => _substanceFilter; set { _substanceFilter = value; NotifyOfPropertyChange(() => SubstanceFilter); UpdateSearchList(); } }
        public IList<string> FoundSubstances { get => _foundSubstances; set { _foundSubstances = value; NotifyOfPropertyChange(() => FoundSubstances); } }
        public string NewSubstance { get => _newSubstance; set { _newSubstance = value; NotifyOfPropertyChange(() => NewSubstance); } }

        string _substanceFilter = "";
        IList<string> _foundSubstances = new List<string>();
        string _newSubstance;
        ChemSepAdapter _db = new ChemSepAdapter();


        public IList<Substance> Components
        {
            get
            {
                return _currentPropertyBlock?.Components;
            }
        }

        public IList<PureEnthalpyFunction> EnthalpyMethods
        {
            get
            {
                return _currentPropertyBlock?.EnthalpyMethod.PureComponentEnthalpies;
            }
        }
        IList<Tuple<Substance, Variable>> _constants;

        public IList<Tuple<Substance, Variable>> Constants
        {
            get { return _constants; }
        }
        IList<Tuple<Substance, TemperatureDependentPropertyFunction>> _functions;

        public IList<Tuple<Substance, TemperatureDependentPropertyFunction>> Functions
        {
            get { return _functions; }
        }

        void UpdateSearchList()
        {
            FoundSubstances = _db.ListComponents(SubstanceFilter);

        }
        public void AddSubstance()
        {
            if (NewSubstance != null && CurrentPropertyBlock != null)
            {
                var substance = _db.FindComponent(NewSubstance);
                if (substance != null)
                {
                    CurrentPropertyBlock.AddComponent(substance);
                    CurrentPropertyBlock.BinaryParameters.Clear();
                    _db.FillBIPs(CurrentPropertyBlock);
                    var test = CurrentPropertyBlock;
                    CurrentPropertyBlock = null;
                    CurrentPropertyBlock = test;

                    NotifyOfPropertyChange(() => Components);
                    NotifyOfPropertyChange(() => EnthalpyMethods);
                    NotifyOfPropertyChange(() => Functions);
                    NotifyOfPropertyChange(() => Constants);



                }
            }
        }


        void ChangePropertyBlock()
        {
            if (CurrentPropertyBlock != null)
            {
                _constants = new List<Tuple<Substance, Variable>>();
                foreach (var sub in CurrentPropertyBlock.Components)
                    foreach (var constant in sub.Constants)
                        _constants.Add(new Tuple<Substance, Variable>(sub, constant));

                _functions = new List<Tuple<Substance, TemperatureDependentPropertyFunction>>();
                foreach (var sub in CurrentPropertyBlock.Components)
                    foreach (var function in sub.Functions)
                        _functions.Add(new Tuple<Substance, TemperatureDependentPropertyFunction>(sub, function));


            


            }
            NotifyOfPropertyChange(() => Components);
            NotifyOfPropertyChange(() => EnthalpyMethods);
            NotifyOfPropertyChange(() => Functions);
            NotifyOfPropertyChange(() => Constants);
        }


        public void AddPropertyBlock()
        {
            AvailablePropertyBlocks.Add(new ThermodynamicSystem("New System", "Ideal"));
        }

        public PropertyManagerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            AvailablePropertyBlocks = new ObservableCollection<ThermodynamicSystem>();

        }

    }
}
