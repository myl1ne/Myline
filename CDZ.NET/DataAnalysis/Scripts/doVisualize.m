clear; clc;


%% PARAMETERS

filename     = 'D:\robotology\src\Myline\CDZ.NET\CDZ.NET\VowelWorldModel\bin\Release\errorLog.csv'; %%%%%%%% CHANGE APPROPRIATELY

numSteps     = 10; % 5000;
offset       = 1; % Because the first column is the timestep;

ORIENT_FACTOR = 180; % We assume that 1.0 means 180 degrees, 0.5 means 90 degrees, etc.

numX     = 3;
numY     = 3;
numColor = 4;

%               R G B    R G B    R G B    R G B
colors     = { [1 0 0]  [0 0 1]  [1 1 0]  [0 1 0] }; % Here, we assume the order of color channels is red, blue, yellow, green.
colorNames = { 'RED'    'BLUE'   'YELLOW' 'GREEN'};


%% INITIALIZATION

data = csvread(filename); % I removed the first row (i.e. the column headings, because "csvread" needs the whole file to be numbers-only.
display(data);
realColor  = NaN(numX, numY, numColor, numSteps); % Multi-dimensional array
realOrient = NaN(numX, numY, numSteps);


%% READ IN THE REAL COLORS

for tempStep = 1 : numSteps
    for tempColor = 1 : numColor
        for tempY = 1 : numY
            for tempX = 1 : numX
                
                tempTemp = offset +  tempColor  +  ((tempY-1) * numColor)  +  ((tempX-1) * numColor * numY);
                realColor(tempX, tempY, tempColor, tempStep) = data(tempStep, tempTemp);
                
            end % for tempX = 1 : numX
        end % for tempY = 1 : numY
    end % for tempColor = 1 : numColor
end % for tempStep = 1 : numSteps

lastColumnUsed = tempTemp; clear tempTemp;
clear tempStep tempColor tempY tempX; % This line isn't necessary at all in Matlab, but I like to do it anyway.

% Error-checking (make sure all entries of the array were filled in, i.e. no longer have the value "NaN")
if (any(isnan(realColor(:))))
    fprintf('PROBLEM QEASDLKQWE!!!\n\n');
    keyboard;
end


%% READ IN THE REAL ORIENTATIONS

for tempStep = 1 : numSteps
    for tempY = 1 : numY
        for tempX = 1 : numX
            
            tempTemp = lastColumnUsed +  tempY  +  ((tempX-1) * numY);
            realOrient(tempX, tempY, tempStep) = data(tempStep, tempTemp);
            
        end % for tempX = 1 : numX
    end % for tempY = 1 : numY
end % for tempStep = 1 : numSteps

lastColumnUsed = tempTemp; clear tempTemp;
clear tempStep tempY tempX;

% Error-checking (make sure all entries of the array were filled in, i.e. no longer have the value "NaN")
if (any(isnan(realOrient(:))))
    fprintf('PROBLEM ASLCMWLKEQW!!!\n\n');
    keyboard;
end

realOrient = realOrient * ORIENT_FACTOR;


%% DISPLAY COLORS

figure; % Open a new figure window;

numRows = 2; %%%%%%%% First row is real input values.
numColumns = numColor + 1;

for curStep = 1 : numSteps
    
    clf; % Clear the figure contents
    set(gcf, 'Name', sprintf('TIME STEP = %d', curStep));
    
    % Display the color channels
    for tempColor = 1 : numColor
        
        subplotNum = tempColor;
        subplot(numRows, numColumns, subplotNum);
        
        tempImg = NaN(numX, numY, 3);
        for tempRGB = 1 : 3
            tempImg(:, :, tempRGB) = realColor(:, :, tempColor, curStep) * colors{tempColor}(tempRGB); % Modulate pure RGBY by multiplying with channel activity.
        end
        clear tempRGB;
        
        % Display the image
        tempImg = permute(tempImg, [2 1 3 4]); % For display purposes, swap X and Y. Because arrays are (row x column), not (X x Y).
        imagesc(tempImg); hold on; axis xy;
        clear tempImg;
        
        % Set axis tick-mark spacing
        set(gca, 'XTick', 1 : numX); set(gca, 'YTick', 1 : numY);
        
        % Display axis labels
        title(sprintf('%s channel', colorNames{tempColor}), 'FontSize', 16);
        if (subplotNum == 1), ylabel('REAL VALUES', 'FontSize', 16); end
        set(gca, 'FontSize', 12);
        
        % Draw grid lines manually
        for tempA = 1.5 : numX-0.5
            plot([tempA tempA], [0.5 numY+0.5], '-', 'Color', [.5 .5 .5]);
        end
        for tempB = 1.5 : numY-0.5
            plot([0.5 numX+0.5], [tempB tempB], '-', 'Color', [.5 .5 .5]);
        end
        clear tempA tempB;
        
    end % for tempColor = 1 : numColor
    
    % Display the orientation
    subplot(numRows, numColumns, numColumns);
    for tempX = 1 : numX
        for tempY = 1 : numY
            o = realOrient(tempX, tempY, curStep);
            % Note that we assume 0 degrees is North, and increases in clock-wise direction.
            plot(tempX + [-0.5 +0.5], tempY + [ min(+0.5, max(-0.5, -0.5*tand(o)))  min(+0.5, max(-0.5, +0.5*tand(o))) ], 'k-', 'LineWidth', 4.0); hold on;
            clear o;
        end % for tempY = 1 : numY
    end % for tempX = 1 : numX
    clear tempX tempY;

    % Set up the axes, tick marks, etc.
    xlim([0.5 numX+0.5]); ylim([0.5 numY+0.5]);
    set(gca, 'XTick', 1 : numX); set(gca, 'YTick', 1 : numY);
    title('ORIENTATION', 'FontSize', 16);
    set(gca, 'FontSize', 12);
    box on;

    
    % Draw grid lines manually
    for tempA = 1.5 : numX-0.5
        plot([tempA tempA], [0.5 numY+0.5], '-', 'Color', [.5 .5 .5]);
    end
    for tempB = 1.5 : numY-0.5
        plot([0.5 numX+0.5], [tempB tempB], '-', 'Color', [.5 .5 .5]);
    end
    clear tempA tempB;
    
    % Wait for user to press any key before going to next time step
    pause;
    
end % for curStep = 1 : numSteps
clear curStep tempColor;


%%



