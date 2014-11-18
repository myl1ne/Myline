#ifndef __DCDZ_POOL_H__
#define __DCDZ_POOL_H__

#include "Neuron.h"

#include "Callbacks.h"

#include <deque>
#include <map>
#include <algorithm>
#include <iostream>

namespace DeepCDZ
{
	//A group of (possibly) interconnected neurons
	class Pool
	{

	protected:
		//An unordered list of neurons. Position and iterators are not guaranted to remain the same.
		std::deque<Neuron*> neurons;

		//A matrix storing the connections between neurons. Indexed by neurons.
		std::map < Neuron*, std::map<Neuron*, Connection*> > connections;

	public:

		//------------------------------
		//Create an empty pool of neurons
		Pool();

		//------------------------------
		//Delete the pool, wiping out all the content
		~Pool();

		//------------------------------
		//Clear the pool, calling destructor of every neuron && connection
		void clear();

		//Add a neuron to the pool based on a given factory
		Neuron* addNeuron(Callback_NeuronFactory factory);

		//Add a neuron to the pool
		void addNeuron(Neuron* n);

		//Add a given number of neurons to the pool
		//@param factory The neuron factory to be used
		//@param count The number of neurons to be added
		//@return a deque of pointer to the neurons created
		std::deque<Neuron*> addNeurons(Callback_NeuronFactory factory, const unsigned int &count);

		//Retrieve the neurons of this pool
		//@return a deque of all the neurons of this pool
		std::deque<Neuron*> getNeurons(){ return neurons; };

		//Update the neurons
		//@param randomOrder Should the update order be random ? False by default
		virtual void update(bool randomOrder = false);

		//Register a given connection within the network.
		//@param c The connection to be added
		//@return true/false in case of success/failure (e.g neurons are not registered with the pool)
		bool addConnection(Connection* c);

		//Create random connections among a set of neurons.
		//@param nodes The set of neurons to be connected.
		//@param connectionProbability Probability that 2 neurons will be connected.
		//@param minimumWeight minimum weight for the initialisation of each connection.
		//@param maximumWeight maximum weight for the initialisation of each connection.
		//@return true/false in case of success/failure (e.g neurons are not registered with the pool)
		std::deque<Connection*> addConnections(std::deque<Neuron*> nodes, double connectionProbability = 1.0, double minimumWeight = 0.0, double maximumWeight = 1.0);

		//Assign a given activity to all the neurons of the pool
		void operator=(const double &value);
	};

}


#endif

