#include "Pool.h"

namespace DeepCDZ
{
	Pool::Pool()
	{
		
	}

	Pool::~Pool()
	{
		this->clear();
	}

	void Pool::clear()
	{
		//delete connections
		for (std::deque<Neuron*>::iterator it = neurons.begin(); it != neurons.end(); it++)
		{
			for (std::deque<Neuron*>::iterator it2 = neurons.begin(); it2 != neurons.end(); it2++)
			{
				delete connections[*it][*it2];
			}
			delete (*it);
		}
		connections.clear();

		//delete neurons
		for (std::deque<Neuron*>::iterator it = neurons.begin(); it != neurons.end(); it++)
			delete (*it);
		neurons.clear();
	}

	void Pool::addNeuron(Neuron* n)
	{
		neurons.push_back(n);
	}
		
	std::deque<Neuron*> Pool::addNeurons(const Neuron &n, const unsigned int &count)
	{
		std::deque<Neuron*> newPool(count);
		for(unsigned int i=0; i<count; i++)
		{
			Neuron* ptr = n.spawn();
			addNeuron(ptr);
			newPool.push_back(ptr);
		}
		return newPool;
	}

	void Pool::update(bool randomOrder)
	{
		if (randomOrder)
			std::random_shuffle ( neurons.begin(), neurons.end() );

		for (std::deque<Neuron*>::iterator it = neurons.begin(); it != neurons.end(); it++)
			(*it)->update();
	}

	void Pool::operator=(const double &value)
	{		
		for (std::deque<Neuron*>::iterator it = neurons.begin(); it != neurons.end(); it++)
			(*it)->activity = value;
	}
}