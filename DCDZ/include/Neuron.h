#ifndef __DCDZ_NEURON_H__
#define __DCDZ_NEURON_H__

#include "Connection.h"

#include <list>

namespace DeepCDZ
{
	
	class Neuron
	{
		public:
			double activity;
			std::list<Connection*> out;
			std::list<Connection*> in;

			//------------------------------
			//Create a neuron
			Neuron();
			
			//------------------------------
			//Spawn a neuron of this specific type
			virtual Neuron* spawn() const;


			//------------------------------
			//Update the activity of the neuron ( sum(inputs x weight) by default  )
			virtual void update();
	};

}


#endif

