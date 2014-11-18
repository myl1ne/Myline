
#include "Fiber.h"

#include "yarp/os/all.h"
#include "yarp/sig/all.h"

using namespace DeepCDZ;

class SpecialNeuron :public Neuron
{
public:
	double specialField;
	SpecialNeuron() :Neuron(){ specialField = 0.0; }
};


Neuron* myNeuronFactory() { return new Neuron(); }
Neuron* mySpecialNeuronFactory() { return new SpecialNeuron(); }

int main()
{
	Fiber* myFiber = new Fiber();
	std::cout << "Adding layer... " << std::endl;
	myFiber->addLayer(640, 480, myNeuronFactory);
	std::cout << "Adding layer... " << std::endl;
	myFiber->addLayer(640, 480, mySpecialNeuronFactory);
	std::cout << "Adding layer... " << std::endl;
	myFiber->addLayer(1, 1, mySpecialNeuronFactory);
	std::cout << "Connecting layers... " << std::endl;
	myFiber->connectLayers(0, 1, Fiber::InterlayerTopology::TopographicalToroidal, 1/640.0);
	myFiber->connectLayers(1, 2, Fiber::InterlayerTopology::All2All, 1.0);

	std::cout << "Starting simulation " << std::endl;
	for (int cycle = 0; cycle < 100; cycle++)
	{
		std::cout << "Cycle " << cycle << std::endl;
		myFiber->update();
	}
}