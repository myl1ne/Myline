%Just a dummy function to demonstrate the available fields

function stemName = convergence_divergence(stemName)
	%Since we cannot pass parameters by reference in matlab we have to return the argument


	for i=1:length(stemName.modalities)

		mod = stemName.modalities{i}

	%Just display the content of the modality
		disp( strcat('Treating modality ', mod.name))
		disp('Real value:')
		disp(mod.reality)
		disp('Influence:')
		disp(mod.influence)

	%Here we are expected to do some smart computation and provide a prediction
		stemName.modalities{i}.prediction = mod.reality

	end
end