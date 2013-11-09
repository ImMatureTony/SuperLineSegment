using UnityEngine;

using System;

static class ObstacleData
{
	public static String easyObstacles =
		@"<CHUNKS> 
			<CHUNK>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.1</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.2</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.3</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.4</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.5</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EndPiece</SHAPE>
					<TIMING>0.6</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
			</CHUNK>
			
			<CHUNK>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0.1</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0.2</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0.3</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EndPiece</SHAPE>
					<TIMING>0.4</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
			</CHUNK>
		</CHUNKS>"		
	;
	
	public static string mediumObstacles = 
		@"<CHUNKS>
			<CHUNK>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>HardTriangle</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0.12</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>HardTriangle</SHAPE>
					<TIMING>0.12</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EndPiece</SHAPE>
					<TIMING>0.4</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
			</CHUNK>
			
			<CHUNK>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>HardTriangle</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EndPiece</SHAPE>
					<TIMING>0.2</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
			</CHUNK>
			
			<CHUNK>
				<OBSTACLE>
					<SHAPE>EasyTriangle</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>HardTriangle</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EndPiece</SHAPE>
					<TIMING>0.2</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
			</CHUNK>
			
			<CHUNK>	
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.025</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.075</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.1</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.15</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.175</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.225</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.25</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EndPiece</SHAPE>
					<TIMING>0.275</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
			</CHUNK>
		</CHUNKS>"
	;

	public static String hardObstacles =
		@"<CHUNKS>
			<CHUNK>	
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.025</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.05</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.075</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.1</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.125</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.15</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.175</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.2</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
					<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.225</TIMING>
					<SIDE>left</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.25</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.275</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
				<OBSTACLE>
					<SHAPE>EasyRect</SHAPE>
					<TIMING>0.3</TIMING>
					<SIDE>right</SIDE>
				</OBSTACLE>
			</CHUNK>	
		</CHUNKS>"
	;
}