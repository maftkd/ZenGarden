//Sand script
//
//This bad boy can generate a rectangular mesh as needed
//The mesh is centered about the Transform.
//In general the mesh's horizontal axis aligns with the world's X, and the vertical axis aligns with the world's Z
//In some cases this may be confusing... Like _size.y actually refers to the length in the Z axis... sorry about that...
//
//There's also some basic ray casting of the mouse/input vector to the ground plane... The code assumes that this plane
//sits at a height of 0 in order for those ray-casts to work... Note also that they don't take any deformations of the mesh
//into account...
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : MonoBehaviour
{
	//mesh
	public Vector2 _size;
	public int _vertsX;
	public int _vertsZ;
	MeshFilter _meshF;
	MeshRenderer _meshR;
	Mesh _mesh;
	float _xMin,_xMax,_zMin,_zMax;
	Vector2 _vertSpacing;

	//drawing
	bool _drawing;
	Camera _cam;
	Vector3 _hitPos;
	public float _drawDiameter;
	public float _drawDepth;
	public AnimationCurve _fallOff;
	bool _penDown;
	public Vector2 _drawDir;
	Vector3 _cursor;
	public float _cursorLerp;
	public float _minDistToUpdate;
	public float _minDistForLine;
	float _prevXFrac, _prevZFrac;

	//audio
	public SandAudio _sandAudio;
	public float _maxVel;
	Vector3 _prevPos;

	//vfx
	public ParticleSystem _sandParts;

	//patterns
	public Material [] _patterns;
	int _lastPattern;

	Help _help;

	void Awake(){
		Init();
		//RaiseLineFrom(1,0,0.5f,0.5f,_drawDiameter,Vector2.right);
		//RaiseLineFrom(0.5f,0.5f,1f,0f,_drawDiameter,Vector2.up);
		//RaiseLineFrom(0f,1f,0.5f,0.75f,_drawDiameter,Vector2.up);
		//RaiseLineFrom(0f,1f,0.5f,0.25f,_drawDiameter,Vector2.up);
		//RaiseLineFrom(0f,0.5f,0f,1.0f,_drawDiameter,Vector2.up);
		UpdateMeshData();
	}

	[ContextMenu("Reset")]
	public void Init(){
		//get references
		_meshF=GetComponent<MeshFilter>();
		_meshR=GetComponent<MeshRenderer>();
		_cam=Camera.main;
		_help=FindObjectOfType<Help>();
		//generate initial mesh
		GenerateMesh();
		LoadPattern(0);
	}

	Vector3[] _verts;
	Vector3[] _norms;
	Vector2[] _uvs;
	int[] _tris;
	public void GenerateMesh(){
		//get spacing between verts in world space units
		float vertXSpacing=_size.x/(_vertsX-1);
		float vertZSpacing=_size.y/(_vertsZ-1);
		_vertSpacing=new Vector2(vertXSpacing,vertZSpacing);
		int numVerts=_vertsX*_vertsZ;
		//validate
		if(_vertsX<2 || _vertsZ<2){
			Debug.Log("Cannot make mesh with less than 2 verts in a direction");
			return;
		}
		//allocate some memory
		_verts = new Vector3[numVerts];
		int numTris=(_vertsX-1)*(_vertsZ-1)*6;
		_tris = new int[numTris];
		_uvs = new Vector2[numVerts];
		_norms = new Vector3[numVerts];

		//get bounds
		_xMin=-_size.x*0.5f;
		_xMax=-_xMin;
		_zMin=-_size.y*0.5f;
		_zMax=-_zMin;
		
		//assign vertex positions - note that these are in local space
		//so when doing ray-casting or debugging points otta be converted to/from world-space
		for(int z=0; z<_vertsZ; z++){
			float zFrac=z/(float)(_vertsZ-1);
			float zPos=Mathf.Lerp(_zMin,_zMax,zFrac);
			for(int x=0; x<_vertsX; x++){
				float xFrac=x/(float)(_vertsX-1);
				float xPos=Mathf.Lerp(_xMin,_xMax,xFrac);
				int vertIndex=z*_vertsX+x;
				_verts[vertIndex]=new Vector3(xPos,0,zPos);
				_uvs[vertIndex]=new Vector2(xFrac,zFrac);
				//#temp
				_norms[vertIndex]=Vector3.up;
			}
		}

		int triCounter=0;
		for(int z=0; z<_vertsZ-1; z++){
			for(int x=0; x<_vertsX-1; x++){
				int vertIndex=z*_vertsX+x;
				//first tri
				_tris[triCounter]=vertIndex;
				_tris[triCounter+1]=vertIndex+_vertsX;
				_tris[triCounter+2]=vertIndex+1;
				//second
				_tris[triCounter+3]=vertIndex+_vertsX;
				_tris[triCounter+4]=vertIndex+1+_vertsX;
				_tris[triCounter+5]=vertIndex+1;
				triCounter+=6;
			}
		}

		_mesh = new Mesh();
		UpdateMeshData();
	}

	Vector2[] _uvCache;
	public void CacheCurPattern(){
		Material patternMat = _patterns[_lastPattern];
		Texture2D patternTex = (Texture2D)patternMat.GetTexture("_MainTex");
		Color[] pixels = patternTex.GetPixels();
		if(_uvCache==null)
			_uvCache=new Vector2[_uvs.Length];
		for(int z=0;z<_vertsZ; z++){
			float normZ=z/(float)(_vertsZ-1);
			int sampleY=Mathf.FloorToInt(normZ*patternTex.height);
			if(sampleY>=patternTex.height)
				sampleY=patternTex.height-1;
			for(int x=0;x<_vertsX; x++){
				int vertIndex=z*_vertsX+x;
				float normX=x/(float)(_vertsX-1);
				int sampleX=Mathf.FloorToInt(normX*patternTex.width);
				if(sampleX>=patternTex.width)
					sampleX=patternTex.width-1;
				int colorIndex=sampleY*patternTex.width+sampleX;
				float color=pixels[colorIndex].r;
				float u = normX;
				if(color>0.5f)
					u+=1f;
				_uvCache[vertIndex].x=u;
				_uvCache[vertIndex].y=normZ;
			}
		}
	}

	public void CacheNextPattern(){
		//go next pattern
		_lastPattern++;
		if(_lastPattern>=_patterns.Length-1)
			_lastPattern=0;
		Material patternMat = _patterns[_lastPattern];
		Texture2D patternTex = (Texture2D)patternMat.GetTexture("_MainTex");
		Color[] pixels = patternTex.GetPixels();
		if(_uvCache==null)
			_uvCache=new Vector2[_uvs.Length];
		for(int z=0;z<_vertsZ; z++){
			float normZ=z/(float)(_vertsZ-1);
			int sampleY=Mathf.FloorToInt(normZ*patternTex.height);
			if(sampleY>=patternTex.height)
				sampleY=patternTex.height-1;
			for(int x=0;x<_vertsX; x++){
				int vertIndex=z*_vertsX+x;
				float normX=x/(float)(_vertsX-1);
				int sampleX=Mathf.FloorToInt(normX*patternTex.width);
				if(sampleX>=patternTex.width)
					sampleX=patternTex.width-1;
				int colorIndex=sampleY*patternTex.width+sampleX;
				float color=pixels[colorIndex].r;
				float u = normX;
				if(color>0.5f)
					u+=1f;
				_uvCache[vertIndex].x=u;
				_uvCache[vertIndex].y=normZ;
			}
		}
	}

	public void CacheLastPattern(){
		//go next pattern
		_lastPattern=_patterns.Length-1;
		Material patternMat = _patterns[_lastPattern];
		Texture2D patternTex = (Texture2D)patternMat.GetTexture("_MainTex");
		Color[] pixels = patternTex.GetPixels();
		if(_uvCache==null)
			_uvCache=new Vector2[_uvs.Length];
		for(int z=0;z<_vertsZ; z++){
			float normZ=z/(float)(_vertsZ-1);
			int sampleY=Mathf.FloorToInt(normZ*patternTex.height);
			if(sampleY>=patternTex.height)
				sampleY=patternTex.height-1;
			for(int x=0;x<_vertsX; x++){
				int vertIndex=z*_vertsX+x;
				float normX=x/(float)(_vertsX-1);
				int sampleX=Mathf.FloorToInt(normX*patternTex.width);
				if(sampleX>=patternTex.width)
					sampleX=patternTex.width-1;
				int colorIndex=sampleY*patternTex.width+sampleX;
				float color=pixels[colorIndex].r;
				float u = normX;
				if(color>0.5f)
					u+=1f;
				_uvCache[vertIndex].x=u;
				_uvCache[vertIndex].y=normZ;
			}
		}
	}

	public void LoadNextPattern(){
		_lastPattern++;
		if(_lastPattern>=_patterns.Length)
			_lastPattern=0;
		LoadPattern(_lastPattern);
	}
	public void LoadPattern(int index){
		_lastPattern=index;
		Material patternMat = _patterns[index];
		Texture2D patternTex = (Texture2D)patternMat.GetTexture("_MainTex");
		Color[] pixels = patternTex.GetPixels();
		for(int z=0;z<_vertsZ; z++){
			float normZ=z/(float)(_vertsZ-1);
			int sampleY=Mathf.FloorToInt(normZ*patternTex.height);
			if(sampleY>=patternTex.height)
				sampleY=patternTex.height-1;
			for(int x=0;x<_vertsX; x++){
				int vertIndex=z*_vertsX+x;
				float normX=x/(float)(_vertsX-1);
				int sampleX=Mathf.FloorToInt(normX*patternTex.width);
				if(sampleX>=patternTex.width)
					sampleX=patternTex.width-1;
				int colorIndex=sampleY*patternTex.width+sampleX;
				float color=pixels[colorIndex].r;
				float u = normX;
				if(color>0.5f)
					u+=1f;
				_uvs[vertIndex].x=u;
			}
		}
		UpdateMeshData();
	}

	public void UpdateMeshData(){
		_mesh.vertices=_verts;
		_mesh.triangles=_tris;
		_mesh.uv=_uvs;
		_mesh.normals=_norms;
		_mesh.RecalculateBounds();
		_meshF.sharedMesh=_mesh;
	}
	
    // Update is called once per frame
    void Update()
    {
		//start drawing on mouse or finger down
		if(Input.GetMouseButtonDown(0)&&!_help.IsActive()){
			Ray r = _cam.ScreenPointToRay(Input.mousePosition);
			//raycast to rocks
			RaycastHit hit;
			if(Physics.Raycast(r.origin,r.direction,out hit,50f,1)){
			}
			else{
				_penDown=true;
			}
			_drawing=true;
		}
		//stop drawing on mouse up
		else if(Input.GetMouseButtonUp(0)){
			_drawing=false;
			_sandParts.Stop();
		}

		//while mouse is down
		if(_drawing){
			Ray r = _cam.ScreenPointToRay(Input.mousePosition);
			//raycast to rocks
			RaycastHit hit;
			if(Physics.Raycast(r.origin,r.direction,out hit,50f,1)){
				_sandAudio.SetTargetVolume(0);
				_sandParts.Stop();
				//# hack
				_penDown=true;
			}
			//check for raking
			else if(Rake._raking||ArpBall._rolling){
				if(_sandParts.isPlaying)
					_sandParts.Stop();
				_sandAudio.SetTargetVolume(0);
				_penDown=false;
				_drawing=false;
			}
			else{
				//raycast to ground plane where y=0
				float t = -r.origin.y/r.direction.y;
				float x = r.origin.x+t*r.direction.x;
				float z = r.origin.z+t*r.direction.z;
				Vector3 worldSpaceHit=new Vector3(x,0,z);
				//cursor snaps on pen down, but moves slower on draw
				if(_penDown)
				{
					_cursor=worldSpaceHit;
					_prevPos=_cursor;
				}
				else//slowly move cursor around
					_cursor=Vector3.Lerp(_cursor,worldSpaceHit,_cursorLerp*Time.deltaTime);
				_hitPos=transform.InverseTransformPoint(_cursor);
				_sandAudio.SetPosition(_cursor);

				//make sure point is within bounds
				if(_hitPos.x>_xMin&&_hitPos.x<_xMax&&_hitPos.z>_zMin&&_hitPos.z<_zMax&&
						!Rock._holding){

					//audio volume calc
					Vector3 diff =_cursor-_prevPos;
					//Vector3 mouseDiff=Input.mousePosition-_prevPos;
					//float xDiff=mouseDiff.x/Screen.width;
					float xDiff=diff.x/_size.x;
					//float zDiff=mouseDiff.y/Screen.height;
					float zDiff=diff.z/_size.y;
					float normDist=Mathf.Sqrt(xDiff*xDiff+zDiff*zDiff);
					float vel = normDist/Time.deltaTime;
					float targetV=vel/_maxVel;
					_sandAudio.SetTargetVolume(targetV);
					_drawDir=new Vector2(xDiff,zDiff);
					
					//deform geometry
					float xFrac = Mathf.InverseLerp(_xMin,_xMax,_hitPos.x);
					float zFrac = Mathf.InverseLerp(_zMin,_zMax,_hitPos.z);
					int xCoord=Mathf.FloorToInt((_vertsX-1)*xFrac);
					int zCoord=Mathf.FloorToInt((_vertsZ-1)*zFrac);
					int vertIndex=zCoord*_vertsX+xCoord;
					if(_penDown)
					{
						_sandParts.Play();
						_sandParts.transform.position=worldSpaceHit;
						RaiseCircleFalloff(vertIndex,_drawDiameter);
					}
					else if(normDist>_minDistToUpdate)
					{
						if(!_sandParts.isPlaying)
						{
							_sandParts.Play();
							_sandParts.transform.position=worldSpaceHit;
						}
						//Debug.Log(normDist);
						if(normDist>_minDistForLine){
							RaiseLineFrom(_prevXFrac,xFrac,_prevZFrac,zFrac,_drawDiameter,_drawDir);
						}
						RaiseHalfCircleFalloff(vertIndex,_drawDiameter,_drawDir);
						//RaiseHalfCircleFalloff(vertIndex,_drawDiameter,_drawDir);
					}
					else
						_sandParts.Stop();

					_sandParts.transform.position=worldSpaceHit;
					_prevXFrac=xFrac;
					_prevZFrac=zFrac;
				}
				else{
					_sandParts.Stop();
					_sandAudio.SetTargetVolume(0);
				}
				_prevPos=_cursor;
				if(_penDown)
					_penDown=false;
			}
		}
		else if(!Rake._raking)
			_sandAudio.SetTargetVolume(0);
		//else
			//_audio.volume=Mathf.Lerp(_audio.volume,0,_audioGravity*Time.deltaTime);
    }

	void LateUpdate(){
		UpdateMeshData();

	}

	Vector3 _debugPos;

	public void RaiseCircleFalloff(Vector3 pos, float diameter){
		float xFrac = Mathf.InverseLerp(_xMin,_xMax,pos.x);
		float zFrac = Mathf.InverseLerp(_zMin,_zMax,pos.z);
		int xCoord=Mathf.FloorToInt((_vertsX-1)*xFrac);
		int zCoord=Mathf.FloorToInt((_vertsZ-1)*zFrac);
		int vertIndex=zCoord*_vertsX+xCoord;
		RaiseCircleFalloff(vertIndex,diameter);
	}

	void RaiseCircleFalloff(int vertIndex, float diameter){
		float radius=diameter*0.5f;
		int xVerts=Mathf.FloorToInt(diameter/_vertSpacing.x);
		int zVerts=Mathf.FloorToInt(diameter/_vertSpacing.y);
		int rowNum=vertIndex/_vertsX;
		int colNum=vertIndex%_vertsX;
		int xMin=colNum-xVerts;
		int xMax=colNum+xVerts;
		int zMin=rowNum-zVerts;
		int zMax=rowNum+zVerts;
		//make sure rect doesn't go out of bounds
		//there is an extra vertex surrounding the region so we can never modify the perimiter vertices
		//this extra vertex is used when calcualting the normal
		if(xMin<1)
			xMin=1;
		if(xMax>_vertsX-2)
			xMax=_vertsX-2;
		if(zMin<1)
			zMin=1;
		if(zMax>_vertsZ-2)
			zMax=_vertsZ-2;

		//modify vertex positions
		for(int z=zMin;z<=zMax;z++){
			float zDiff=Mathf.Abs(z-rowNum)*_vertSpacing.y;
			for(int x=xMin;x<=xMax;x++){
				float xDiff=Mathf.Abs(x-colNum)*_vertSpacing.x;
				//good ol' pythagoras
				float rad=Mathf.Sqrt(zDiff*zDiff+xDiff*xDiff);
				if(rad<=radius){
					float fallOffT=rad/radius;
					int vert=z*_vertsX+x;
					_verts[vert].y=_drawDepth*(_fallOff.Evaluate(fallOffT)-1);
				}
			}
		}

		//update normals in region
		for(int z=zMin;z<=zMax;z++){
			for(int x=xMin;x<=xMax;x++){
				int vert=z*_vertsX+x;
				int left=vert-1;
				int right=vert+1;
				int below=vert-_vertsX;
				int above=vert+_vertsX;
				Vector3 xDiff=_verts[right]-_verts[left];
				Vector3 zDiff=_verts[above]-_verts[below];
				_norms[vert]=Vector3.Cross(zDiff,xDiff);
			}
		}
		
		_debugPos=_verts[vertIndex];
		//UpdateMeshData();
	}

	public void RaiseHalfCircleFalloff(Vector3 worldSpace, float diameter, Vector2 drawDir, 
			bool mixColors=true,bool ignoreAudio=false){
		float xFrac = Mathf.InverseLerp(_xMin,_xMax,worldSpace.x);
		float zFrac = Mathf.InverseLerp(_zMin,_zMax,worldSpace.z);
		int xCoord=Mathf.FloorToInt((_vertsX-1)*xFrac);
		int zCoord=Mathf.FloorToInt((_vertsZ-1)*zFrac);
		int vertIndex=zCoord*_vertsX+xCoord;
		RaiseHalfCircleFalloff(vertIndex,diameter,drawDir,mixColors,ignoreAudio);
	}

	public void RaiseHalfCircleFalloff(int vertIndex, float diameter, Vector2 drawDir,
			bool mixColors=true,bool ignoreAudio=false){
		float radius=diameter*0.5f;
		int xVerts=Mathf.FloorToInt(diameter/_vertSpacing.x);
		int zVerts=Mathf.FloorToInt(diameter/_vertSpacing.y);
		int rowNum=vertIndex/_vertsX;
		int colNum=vertIndex%_vertsX;
		int xMin=colNum-xVerts;
		int xMax=colNum+xVerts;
		int zMin=rowNum-zVerts;
		int zMax=rowNum+zVerts;
		//make sure rect doesn't go out of bounds
		//there is an extra vertex surrounding the region so we can never modify the perimiter vertices
		//this extra vertex is used when calcualting the normal
		if(xMin<1)
			xMin=1;
		if(xMax>_vertsX-2)
			xMax=_vertsX-2;
		if(zMin<1)
			zMin=1;
		if(zMax>_vertsZ-2)
			zMax=_vertsZ-2;

		//modify vertex positions
		int verts=0;
		for(int z=zMin;z<=zMax;z++){
			float zDiff=(z-rowNum)*_vertSpacing.y;
			for(int x=xMin;x<=xMax;x++){
				float xDiff=(x-colNum)*_vertSpacing.x;
				//good ol' pythagoras
				float rad=Mathf.Sqrt(zDiff*zDiff+xDiff*xDiff);
				Vector2 dir = new Vector2(xDiff,zDiff);
				float ang = Vector2.Angle(dir,drawDir);
				//Debug.Log(ang);
				if(rad<=radius&&ang<=90){
					float fallOffT=rad/radius;
					int vert=z*_vertsX+x;
					_verts[vert].y=_drawDepth*(_fallOff.Evaluate(fallOffT)-1+0.5f);
					verts++;
				}
			}
		}
		//Debug.Break();
		//Debug.Log("verts hit: "+verts+", draw dir: "+drawDir.normalized);

		//update normals in region
		//and uvs
		float avgColor=0;
		int totalCols=0;
		for(int z=zMin;z<=zMax;z++){
			float zDiff2=(z-rowNum)*_vertSpacing.y;
			for(int x=xMin;x<=xMax;x++){
				float xDiff2=(x-colNum)*_vertSpacing.x;
				int vert=z*_vertsX+x;
				int left=vert-1;
				int right=vert+1;
				int below=vert-_vertsX;
				int above=vert+_vertsX;
				Vector3 xDiff=_verts[right]-_verts[left];
				Vector3 zDiff=_verts[above]-_verts[below];
				_norms[vert]=Vector3.Cross(zDiff,xDiff);
				float cur = _uvs[vert].x;
				//good ol' pythagoras
				float rad=Mathf.Sqrt(zDiff2*zDiff2+xDiff2*xDiff2);
				//try swapping uvs
				if(mixColors&&rad<=radius)
				{
					float theta=Mathf.PI*2*Random.value;
					rad = Random.value*radius;
					int xOff = Mathf.RoundToInt(Mathf.Cos(theta)*rad/_vertSpacing.x);
					int zOff = Mathf.RoundToInt(Mathf.Sin(theta)*rad/_vertSpacing.y);
					if(x+xOff>0&&x+xOff<_vertsX&&z+zOff>0&&z+zOff<_vertsZ){
						//int randVert=Random.Range(zMin,zMax+1)*_vertsX+Random.Range(xMin,xMax+1);
						int randVert=(z+zOff)*_vertsZ+x+xOff;
						_uvs[vert].x=_uvs[randVert].x;
						_uvs[randVert].x=cur;
					}
				}
				avgColor+=Mathf.Floor(cur);
				_cols+=Mathf.Floor(cur);
				totalCols++;
				_pixels++;
			}
		}

		avgColor/=totalCols;
		if(!ignoreAudio)
			_sandAudio.SetAverageColor(avgColor);
		//_debugPos=_verts[vertIndex];
	}

	float _cols;
	int _pixels;
	float _avg;

	public void ResetAvgSandColor(){
		_cols=0;
		_pixels=0;
	}
	public float GetAvgSandColor(){
		if(_pixels==0)
			return _avg;
		_avg=_cols/_pixels;
		return _avg;
	}

	public void RaiseLineFrom(Vector3 prev, Vector3 cur, 
			float diameter, Vector2 drawDir, bool mixColors=true){
		float prevX=Mathf.InverseLerp(_xMin,_xMax,prev.x);
		float curX=Mathf.InverseLerp(_xMin,_xMax,cur.x);
		float prevZ=Mathf.InverseLerp(_zMin,_zMax,prev.z);
		float curZ=Mathf.InverseLerp(_zMin,_zMax,cur.z);
		RaiseLineFrom(prevX,curX,prevZ,curZ,diameter,drawDir,mixColors);
	}

	//x and z are normalized from 0 to 1
	void RaiseLineFrom(float prevX, float curX, float prevZ, float curZ, 
			float diameter, Vector2 drawDir, bool mixColors=true){
		int startX=Mathf.FloorToInt((_vertsX-1)*prevX);
		int startZ=Mathf.FloorToInt((_vertsZ-1)*prevZ);
		int endX=Mathf.FloorToInt((_vertsX-1)*curX);
		int endZ=Mathf.FloorToInt((_vertsZ-1)*curZ);
		float slope = (float)(endZ-startZ)/(float)(endX-startX);
		//gradual to 45 degree
		if(Mathf.Abs(slope)<=1f){
			int inc = endX>startX? 1 : -1;
			int iDiff=Mathf.Abs(endX-startX);
			for(int i=0; i<=iDiff; i++){
				int x=startX+inc*i;
				//slope = dz/dx
				//dz = slope*dx
				float dz = slope*(x-startX);
				int z = Mathf.RoundToInt(startZ+dz);
				int vertIndex=z*_vertsX+x;
				RaiseHalfCircleFalloff(vertIndex,diameter,drawDir,mixColors,true);
			}
		}
		//steep but not vertical
		else{
			int inc = endZ>startZ? 1 : -1;
			int iDiff=Mathf.Abs(endZ-startZ);
			for(int i=0; i<=iDiff; i++){
				int z=startZ+inc*i;
				//slope = dz/dx
				//dx = dz/slope
				float dx=(z-startZ)/slope;
				int x = Mathf.RoundToInt(startX+dx);
				int vertIndex=z*_vertsX+x;
				RaiseHalfCircleFalloff(vertIndex,diameter,drawDir,mixColors,true);
			}
		}
	}

	public float GetNormalizedZ(float z){
		return Mathf.InverseLerp(_zMin+transform.position.z,_zMax+transform.position.z,z);
	}

	public bool WithinBox(Vector3 v,float buffer){
		return (v.x>_xMin+transform.position.x+buffer&&v.x<_xMax+transform.position.x-buffer&&
				v.z>_zMin+transform.position.z+buffer&&v.z<_zMax+transform.position.z-buffer);
	}

	public Vector3 ClosestPoint(Vector3 v, float buffer){
		if(WithinBox(v,buffer))
			return v;
		float zDiff=0;
		float xDiff=0;
		if(v.z<_zMin+transform.position.z+buffer)
			zDiff=v.z-(_zMin+transform.position.z+buffer);
		else if(v.z>_zMax+transform.position.z-buffer)
			zDiff=v.z-(_zMax+transform.position.z-buffer);
		if(v.x<_xMin+transform.position.x+buffer)
			xDiff=v.x-(_xMin+transform.position.x+buffer);
		else if(v.x>_xMax+transform.position.x-buffer)
			xDiff=v.x-(_xMax+transform.position.x-buffer);


		if(zDiff<0&&Mathf.Abs(xDiff)<=zDiff*-1){
			v.z=_zMin+transform.position.z+buffer;
			float x = Mathf.InverseLerp(_xMin+buffer,_xMax-buffer,v.x);
			v.x=Mathf.Lerp(_xMin+buffer,_xMax-buffer,x);
		}
		else if(xDiff<0&&Mathf.Abs(zDiff)<=xDiff*-1){
			v.x=_xMin+transform.position.x+buffer;
			float z = Mathf.InverseLerp(_zMin+transform.position.z+buffer,_zMax+transform.position.z-buffer,v.z);
			v.z=Mathf.Lerp(_zMin+buffer,_zMax-buffer,z);
			v.z+=transform.position.z;
		}
		else if(zDiff>0&&Mathf.Abs(xDiff)<=zDiff){
			v.z=_zMax+transform.position.z-buffer;
			float x = Mathf.InverseLerp(_xMin+buffer,_xMax-buffer,v.x);
			v.x=Mathf.Lerp(_xMin+buffer,_xMax-buffer,x);
		}
		else if(xDiff>0&&Mathf.Abs(zDiff)<=xDiff){
			v.x=_xMax+transform.position.x-buffer;
			float z = Mathf.InverseLerp(_zMin+transform.position.z+buffer,_zMax+transform.position.z-buffer,v.z);
			v.z=Mathf.Lerp(_zMin+buffer,_zMax-buffer,z);
			v.z+=transform.position.z;
		}
		return v;
	}

	public void Level(float amount){
		for(int z=0; z<_vertsZ;z++)
		{
			for(int x=0; x<_vertsX; x++){
				int vertIndex=z*_vertsX+x;
				Vector3 cur = _verts[vertIndex];
				Vector3 target=cur;
				target.y=0;
				_verts[vertIndex]=Vector3.Lerp(cur,target,amount);
				cur=_norms[vertIndex];
				_norms[vertIndex]=Vector3.Lerp(cur,Vector3.up,amount);
			}
		}
	}

	public void BlendToCachedPattern(float amount){
		for(int z=0; z<_vertsZ;z++){
			for(int x=0; x<_vertsX; x++){
				int vertIndex=z*_vertsX+x;
				_uvs[vertIndex]=Vector2.Lerp(_uvs[vertIndex],_uvCache[vertIndex],amount);
			}
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.TransformPoint(_hitPos),0.1f);
		//Gizmos.color=Color.blue;
		//Gizmos.DrawWireSphere(transform.TransformPoint(_debugPos),_drawRadius);
		Gizmos.color=Color.magenta;
		//Gizmos.DrawWireSphere(_cursor,_drawRadius);
		Gizmos.DrawRay(_cursor,new Vector3(_drawDir.x,0,_drawDir.y).normalized*5f);
	}
}
