#include "Connection.h"

namespace DeepCDZ
{
	Connection::Connection(Neuron* _src, Neuron* _dest, double _weight)
	{
		source = _src;
		target = _dest;
		weight = _weight;
	}

	Connection::Connection(Neuron* _src, Neuron* _dest, double _minWeight, double _maxWeight)
	{
		source = _src;
		target = _dest;
		weight = ( rand() / (double) RAND_MAX ) * (_maxWeight - _minWeight) + _minWeight;
	}
}