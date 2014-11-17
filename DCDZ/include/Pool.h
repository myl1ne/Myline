#ifndef __DCDZ_POOL_H__
#define __DCDZ_POOL_H__

#include "Neuron.h"

#include <deque>
#include <map>
#include <algorithm>    // std::random_shuffle

namespace DeepCDZ
{
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

		//Add a neuron to the pool
		void addNeuron(Neuron* n);

		//Add a given number of neurons to the pool
		//@param n The type of neuron to be copied
		//@param count The number of neurons to be added
		//@return a deque of pointer to the neurons created
		std::deque<Neuron*> addNeurons(const Neuron &n, const unsigned int &count);

		//Update the neurons
		//@param randomOrder Should the update order be random ? False by default
		void update(bool randomOrder = false);

		//Assign a given activity to all the neurons of the pool
		void operator=(const double &value);
	};

}


#endif

