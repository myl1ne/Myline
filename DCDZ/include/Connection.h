#ifndef __DCDZ_CONNECTION_H__
#define __DCDZ_CONNECTION_H__

#include <stdlib.h>

namespace DeepCDZ
{
	//Ancitcipatory declaration
	class Neuron;

	//Manages a connection between two neurons
	class Connection
	{
	public:
		//Weight of the connection
		double weight;
		//Pointer toward the source neuron
		Neuron* source;
		//Pointer toward the target neuron
		Neuron* target;
		
		//------------------------------

		//Create a connection between 2 neurons with a fixed weight
		Connection(Neuron* _src, Neuron* _dest, double _weight);
		
		//Create a connection between 2 neurons with a random weight
		Connection(Neuron* _src, Neuron* _dest, double _minWeight, double _maxWeight);
	};

}


#endif

