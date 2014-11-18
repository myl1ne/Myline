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

	Neuron* Pool::addNeuron(Callback_NeuronFactory factory)
	{
		Neuron* n = factory();
		addNeuron(n);
		return n;
	}

	void Pool::addNeuron(Neuron* n)
	{
		neurons.push_back(n);
	}
		
	std::deque<Neuron*> Pool::addNeurons(Callback_NeuronFactory factory, const unsigned int &count)
	{
		std::deque<Neuron*> newPool(count);
		for(unsigned int i=0; i<count; i++)
		{
			Neuron* ptr = factory();
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
	
	bool Pool::addConnection(Connection* c)
	{
		if (find(neurons.begin(), neurons.end(), c->source) == neurons.end()
			&&
			find(neurons.begin(), neurons.end(), c->target) == neurons.end())
		{
			std::cerr << "Pool::addConnection(): either src or target neuron is not registred in the pool." << std::endl;
			return false;
		}

		if (connections[c->source][c->target] != NULL)
		{
			std::cerr << "Pool::addConnection(): a connection between src and target neurons already exists." << std::endl;
			return false;
		}

		connections[c->source][c->target] = c;
		c->source->out.push_back(c);
		c->target->in.push_back(c);
		return true;
	}

	std::deque<Connection*> Pool::addConnections(std::deque<Neuron*> nodes, double connectionProbability, double minimumWeight, double maximumWeight)
	{
		std::deque<Connection*> newConnections;
		for (std::deque<Neuron*>::iterator it1 = nodes.begin(); it1 != nodes.end(); it1++)
		{
			for (std::deque<Neuron*>::iterator it2 = nodes.begin(); it2 != nodes.end(); it2++)
			{
				double diceResult = rand() / (double)RAND_MAX;
				if (diceResult <= connectionProbability)
				{
					Connection* c = new Connection(*it1, *it2, minimumWeight, maximumWeight);
					addConnection(c);
					newConnections.push_back(c);
				}
			}
		}
		return newConnections;
	}

	void Pool::operator=(const double &value)
	{		
		for (std::deque<Neuron*>::iterator it = neurons.begin(); it != neurons.end(); it++)
			(*it)->activity = value;
	}
}