#include "Neuron.h"
namespace DeepCDZ
{
	Neuron::Neuron()
	{
		activity = 0.0;
	}

	Neuron* spawn()
	{
		return new Neuron();
	}

	void Neuron::update()
	{
		activity = 0.0;
		for(std::list<Connection*>::iterator it= in.begin(); it!= in.end(); it++)
		{
			activity += (*it)->source->activity * (*it)->weight;
		}
		
		//Here we can apply a sigmoid
	}
}