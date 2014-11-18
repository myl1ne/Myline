#include "Fiber.h"

namespace DeepCDZ
{
	Fiber::Fiber()
	{
	
	}

	Fiber::Fiber(std::vector<int> structure, Callback_NeuronFactory factory)
	{
		layers.resize(structure.size());
		for (unsigned int L = 0; L < structure.size(); L++)
		{
			int layerSize = structure[L];
			layers[L] = new Sheet(layerSize, layerSize, factory);
		}
	}

	void Fiber::addLayer(int width, int height, Callback_NeuronFactory factory)
	{
		layers.push_back(new Sheet(width, height, factory));

		std::deque<Neuron*> freshNeurons = layers.back()->getNeurons();
		for (std::deque<Neuron*>::iterator it = freshNeurons.begin(); it != freshNeurons.end(); it++)
		{
			addNeuron(*it);
		}
	}

	void Fiber::update(bool rndOrder)
	{
		for (unsigned int L = 0; L < layers.size(); L++)
		{
			layers[L]->update(rndOrder);
		}
	}

	bool Fiber::connectLayers(unsigned int layer1, unsigned int layer2, InterlayerTopology topology, double density, Callback_1DDouble_1DDouble weightFx)
	{
		if (layer1 >= layers.size() || layer2 >= layers.size())
		{
			std::cerr << "connectLayers() : index out of range." << std::endl;
			return false;
		}
		if (layer1 == layer2)
		{
			std::cerr << "connectLayers() : indexes are the same." << std::endl;
			return false;
		}

		for (int x1 = 0; x1 < layers[layer1]->Width(); x1++)
		{
			for (int y1 = 0; y1 < layers[layer1]->Height(); y1++)
			{
				for (int x2 = 0; x2 < layers[layer2]->Width(); x2++)
				{
					for (int y2 = 0; y2 < layers[layer2]->Height(); y2++)
					{
						//-------------All2All---------------------//
						if (topology == All2All)
						{
							Connection* c = new Connection(layers[layer1]->neuron(x1, y1), layers[layer2]->neuron(x2, y2), 1.0);
							addConnection(c);
						}

						//-------------Random---------------------//
						if (topology == Random)
						{
							if (rand() / (double)RAND_MAX <= density)
							{
								Connection* c = new Connection(layers[layer1]->neuron(x1, y1), layers[layer2]->neuron(x2, y2), 1.0);
								addConnection(c);
							}
						}	

						//-------------TOPOGRAPHICAL---------------------//
						if (topology == TopographicalRectangular || topology == TopographicalToroidal)
						{
							double normX1 = x1 / (double)layers[layer1]->Width();
							double normY1 = y1 / (double)layers[layer1]->Height();
							double normX2 = x2 / (double)layers[layer2]->Width();
							double normY2 = y2 / (double)layers[layer2]->Height();

							double d = sqrt(pow(normX2 - normX1, 2.0) + pow(normY2 - normY1, 2.0));

							if (topology == TopographicalToroidal)
							{
								double dX = abs(normX1 - normX2);
								double dY = abs(normY1 - normY2);
								double tdX = abs(normX1 + (1.0 - normX2));
								double tdY = abs(normY1 + (1.0 - normY2));
								d = sqrt(pow(std::min(dX, tdX), 2.0) + pow(std::min(dY, tdY), 2.0));
							}
							double weight = 1.0;
							if (weightFx != NULL)
								weight = weightFx(d);
							if (d <= density)
							{
								Connection* c = new Connection(layers[layer1]->neuron(x1, y1), layers[layer2]->neuron(x2, y2), weight);
								addConnection(c);
							}
						}
					}
				}
			}
		}
		return true;
	}
}