using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using MiniSim.Core.Numerics;
using MiniSim.Core.Thermodynamics;
using MiniSim.Core.UnitsOfMeasure;

namespace MiniSim.Core.ModelLibrary
{
    public class Neuron
    {
        public Variable Input;
        public Variable Output;
        public Expression ActivationFunction;
        public double Bias = 0.0;

        public Neuron(int layer, int number)
        {
            Input = new Variable("u", 1.0, SI.none);
            Output = new Variable("y", 1.0, SI.none);
            Input.Subscript = $"{layer},{number}";
            Output.Subscript = $"{layer},{number}";
            ActivationFunction = 1.0 / Sym.Par(1 + Sym.Exp(-Input));
        }
    }


    public class NeuralNet : ProcessUnit
    {
        int _numberOfInputs = 0;
        int _numberOfOutputs = 0;
        int _numberOfLayers = 1;

        private Neuron[] _inputs;
        private Neuron[] _outputs;

        public int NumberOfInputs { get => _numberOfInputs; set => _numberOfInputs = value; }
        public int NumberOfOutputs { get => _numberOfOutputs; set => _numberOfOutputs = value; }
        public int NumberOfLayers { get => _numberOfLayers; set => _numberOfLayers = value; }
        public Neuron[] Inputs { get => _inputs; set => _inputs = value; }
        public Neuron[] Outputs { get => _outputs; set => _outputs = value; }

        Neuron[][] _layers;
        double[][,] _weights;

        List<Tuple<int, Expression>> _inputBindings = new List<Tuple<int, Expression>>();
        List<Tuple<int, Expression>> _outputBindings = new List<Tuple<int, Expression>>();

        public NeuralNet(string name, int numInputs, int neuronsPerHiddenLayer, int numOutputs) : this(name, numInputs, new int[] { neuronsPerHiddenLayer }, numOutputs)
        {
        }

        public NeuralNet(string name, int numInputs, int[] neuronsPerHiddenLayer, int numOutputs) : base(name, null)
        {
            Class = "NeuralNet";
            NumberOfInputs = numInputs;
            NumberOfOutputs = numOutputs;
            Inputs = new Neuron[NumberOfInputs];
            Outputs = new Neuron[NumberOfOutputs];
            NumberOfLayers = neuronsPerHiddenLayer.Length;

            _layers = new Neuron[NumberOfLayers][];
            _weights = new double[NumberOfLayers + 1][,];


            for (int i = 0; i < NumberOfInputs; i++)
            {
                Inputs[i] = new Neuron(0, i);

            }
            for (int i = 0; i < NumberOfOutputs; i++)
            {
                Outputs[i] = new Neuron(NumberOfLayers + 1, i);
            }

            AddVariables(Inputs.Select(n => n.Input));
            AddVariables(Inputs.Select(n => n.Output));

            AddVariables(Outputs.Select(n => n.Input));
            AddVariables(Outputs.Select(n => n.Output));

            for (int i = 0; i < NumberOfLayers; i++)
            {
                _layers[i] = new Neuron[neuronsPerHiddenLayer[i]];

                var numLastOutputs = 1;
                if (i == 0)
                    numLastOutputs = NumberOfInputs;
                else
                    numLastOutputs = neuronsPerHiddenLayer[i - 1];

                _weights[i] = new double[neuronsPerHiddenLayer[i], numLastOutputs];

                for (int j = 0; j < neuronsPerHiddenLayer[i]; j++)
                {
                    _layers[i][j] = new Neuron(i+1, j);

                    for (int k = 0; k < numLastOutputs; k++)
                    {
                        _weights[i][j, k] = 1.0;
                    }
                }

                _weights[i + 1] = new double[NumberOfOutputs, neuronsPerHiddenLayer[NumberOfLayers - 1]];

                for (int j = 0; j < NumberOfOutputs; j++)
                {
                    for (int k = 0; k < neuronsPerHiddenLayer[NumberOfLayers - 1]; k++)
                    {
                        _weights[i + 1][j, k] = 1.0;
                    }
                }
                AddVariables(_layers[i].Select(l => l.Input));
                AddVariables(_layers[i].Select(l => l.Output));

            }

        }

        public NeuralNet BindInput(int i, Expression expr)
        {
            _inputBindings.Add(new Tuple<int, Expression>(i, expr));
            return this;
        }

        public NeuralNet BindOutput(int i, Expression expr)
        {
            _outputBindings.Add(new Tuple<int, Expression>(i, expr));
            return this;
        }

        public override void CreateEquations(AlgebraicSystem problem)
        {
            Action<Expression> EQ = (e) => AddEquationToEquationSystem(problem, e);

            for (int j = 0; j < Inputs.Length; j++)
            {                
                EQ(Inputs[j].Output - Inputs[j].ActivationFunction);
            }

            for (int j = 0; j < _layers[0].Length; j++)
            {
                EQ(_layers[0][j].Input - Sym.Par(Sym.Sum(0, NumberOfInputs, k => _weights[0][j, k] * Inputs[k].Output) + _layers[0][j].Bias));
                EQ(_layers[0][j].Output - _layers[0][j].ActivationFunction);
            }

            for (int i = 1; i < NumberOfLayers; i++)
            {
                for (int j = 0; j < _layers[i].Length; j++)
                {
                    EQ(_layers[i][j].Input - Sym.Par(Sym.Sum(0, _layers[i - 1].Length, k => _weights[i][j, k] * _layers[i - 1][k].Output) + _layers[i][j].Bias));
                    EQ(_layers[i][j].Output - _layers[i][j].ActivationFunction);
                }
            }

            for (int j = 0; j < Outputs.Length; j++)
            {
                EQ(Outputs[j].Input - Sym.Sum(0, _layers[NumberOfLayers - 1].Length, k => _weights[NumberOfLayers][j, k] * _layers[NumberOfLayers - 1][k].Output));
                EQ(Outputs[j].Output - Outputs[j].ActivationFunction);
            }

            foreach (var binding in _inputBindings)
            {
                EQ(Inputs[binding.Item1].Input - binding.Item2);
            }
            foreach (var binding in _outputBindings)
            {
                EQ(Outputs[binding.Item1].Output - binding.Item2);
            }

            base.CreateEquations(problem);
        }

    }
}
