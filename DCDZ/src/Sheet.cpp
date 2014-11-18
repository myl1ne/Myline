#include "Sheet.h"

namespace DeepCDZ
{
	Sheet::Sheet(int _w, int _h, Callback_NeuronFactory factory) :Pool()
	{
		width = _w;
		height = _h;

		neurons2D.resize(width);
		for (int x1 = 0; x1 < width; x1++)
		{
			neurons2D[x1].resize(height);			
			for (int y1 = 0; y1 < height; y1++)
			{
				neurons2D[x1][y1] = addNeuron(factory);
			}
		}
	}

	void Sheet::CreateLateralConnections(int radius, Topology topology, Callback_1DDouble_1DDouble weightFx)
	{
		for (int x1 = 0; x1 < width; x1++)
		{
			for (int y1 = 0; y1 < height; y1++)
			{
				for (int x2 = 0; x2 < width; x2++)
				{
					for (int y2 = 0; y2 < height; y2++)
					{
						double d = sqrt(pow(x2 - x1, 2.0) + pow(y2 - y1, 2.0));
						
						if (topology == Topology::Toroidal)
						{
							double dX = abs(x1 - x2);
							double dY = abs(y1 - y2);
							double tdX = abs(x1 + (width - x2));
							double tdY = abs(y1 + (height - y2));
							d = sqrt(pow(std::min(dX, tdX), 2.0) + pow(std::min(dY, tdY), 2.0));
						}

						double w;
						if (weightFx == NULL)
							w = rand() / (double)RAND_MAX;
						else
							w = weightFx(d);

						if (d <= radius)
						{
							Neuron* src = neuron(x1, y1);
							Neuron* dest = neuron(x2, y2);
							Connection *c = new Connection(src, dest, w);
							addConnection(c);
						}
					}
				}
			}
		}
	}

}