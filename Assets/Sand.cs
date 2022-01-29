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
//Currently this script controls an attached audio source, however, the audio system is probably going to change so this is
//just temporary

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
	public float _drawRadius;
	public AnimationCurve _fallOff;

	//audio
	AudioSource _audio;
	public float _maxVel;
	float _targetVolume;
	Vector3 _prevPos;
	[Tooltip("The lerp factor for audio volume to fall to zero when not drawing")]
	public float _audioGravity;
	[Tooltip("The lerp factor for audio volume to update while drawing")]
	public float _audioSensitivity;

	//vfx
	public ParticleSystem _sandParts;

	void Awake(){
		Init();
	}

	[ContextMenu("Reset")]
	public void Init(){
		//get references
		_meshF=GetComponent<MeshFilter>();
		_meshR=GetComponent<MeshRenderer>();
		_cam=Camera.main;
		_audio=GetComponent<AudioSource>();
		_audio.volume=0;
		//generate initial mesh
		GenerateMesh();
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

	void UpdateMeshData(){
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
		//start drawing on mouse down
		if(Input.GetMouseButtonDown(0)){
			_drawing=true;
			_prevPos=Input.mousePosition;
			_sandParts.Play();
		}
		//stop drawing on mouse up
		else if(Input.GetMouseButtonUp(0)){
			_drawing=false;
			_sandParts.Stop();
		}

		//while mouse is down
		if(_drawing){
			//raycast to ground plane where y=0
			Ray r = _cam.ScreenPointToRay(Input.mousePosition);
			float t = -r.origin.y/r.direction.y;
			float x = r.origin.x+t*r.direction.x;
			float z = r.origin.z+t*r.direction.z;
			//convert world space to local
			Vector3 worldSpaceHit=new Vector3(x,0,z);
			_hitPos=transform.InverseTransformPoint(worldSpaceHit);

			//make sure point is within bounds
			if(_hitPos.x>_xMin&&_hitPos.x<_xMax&&_hitPos.z>_zMin&&_hitPos.z<_zMax){
				//determine vertex index closest to raycast hit
				float xFrac = Mathf.InverseLerp(_xMin,_xMax,_hitPos.x);
				float zFrac = Mathf.InverseLerp(_zMin,_zMax,_hitPos.z);
				int xCoord=Mathf.FloorToInt((_vertsX-1)*xFrac);
				int zCoord=Mathf.FloorToInt((_vertsZ-1)*zFrac);
				int vertIndex=zCoord*_vertsX+xCoord;
				//deform geometry
				RaiseCircleFalloff(vertIndex,_drawRadius);
				
				//audio calc
				Vector3 mouseDiff=Input.mousePosition-_prevPos;
				float xDiff=Mathf.Abs(mouseDiff.x)/Screen.width;
				float zDiff=Mathf.Abs(mouseDiff.y)/Screen.height;
				float normDist=Mathf.Sqrt(xDiff*xDiff+zDiff*zDiff);
				float vel = normDist/Time.deltaTime;
				_targetVolume=vel/_maxVel;
				_audio.volume=Mathf.Lerp(_audio.volume,_targetVolume,_audioSensitivity*Time.deltaTime);

				_sandParts.transform.position=worldSpaceHit;
			}

			_prevPos=Input.mousePosition;
		}
		else
			_audio.volume=Mathf.Lerp(_audio.volume,0,_audioGravity*Time.deltaTime);
        
    }

	void RaiseVert(int vertIndex){
		_verts[vertIndex].y=0.5f;
		UpdateMeshData();
	}

	void RaiseRect(int vertIndex, float radius){
		int xVerts=Mathf.FloorToInt(radius/_vertSpacing.x);
		int zVerts=Mathf.FloorToInt(radius/_vertSpacing.y);
		int rowNum=vertIndex/_vertsX;
		int colNum=vertIndex%_vertsX;
		int xMin=colNum-xVerts;
		int xMax=colNum+xVerts;
		int zMin=rowNum-zVerts;
		int zMax=rowNum+zVerts;
		//make sure rect doesn't go out of bounds
		if(xMin<0)
			xMin=0;
		if(xMax>_vertsX-1)
			xMax=_vertsX-1;
		if(zMin<0)
			zMin=0;
		if(zMax>_vertsZ-1)
			zMax=_vertsZ-1;

		for(int z=zMin;z<=zMax;z++){
			for(int x=xMin;x<=xMax;x++){
				int vert=z*_vertsX+x;
				_verts[vert].y=0.5f;
			}
		}
		UpdateMeshData();
	}

	Vector3 _debugPos;
	void RaiseCircle(int vertIndex, float radius){
		int xVerts=Mathf.FloorToInt(radius/_vertSpacing.x);
		int zVerts=Mathf.FloorToInt(radius/_vertSpacing.y);
		int rowNum=vertIndex/_vertsX;
		int colNum=vertIndex%_vertsX;
		int xMin=colNum-xVerts;
		int xMax=colNum+xVerts;
		int zMin=rowNum-zVerts;
		int zMax=rowNum+zVerts;
		//make sure rect doesn't go out of bounds
		if(xMin<0)
			xMin=0;
		if(xMax>_vertsX-1)
			xMax=_vertsX-1;
		if(zMin<0)
			zMin=0;
		if(zMax>_vertsZ-1)
			zMax=_vertsZ-1;

		for(int z=zMin;z<=zMax;z++){
			float zDiff=Mathf.Abs(z-rowNum)*_vertSpacing.y;
			for(int x=xMin;x<=xMax;x++){
				float xDiff=Mathf.Abs(x-colNum)*_vertSpacing.x;
				//good ol' pythagoras
				float rad=Mathf.Sqrt(zDiff*zDiff+xDiff*xDiff);
				if(rad<=radius){
					int vert=z*_vertsX+x;
					_verts[vert].y=0.5f;
				}
			}
		}
		_debugPos=_verts[vertIndex];
		UpdateMeshData();
	}

	void RaiseCircleFalloff(int vertIndex, float radius){
		int xVerts=Mathf.FloorToInt(radius/_vertSpacing.x);
		int zVerts=Mathf.FloorToInt(radius/_vertSpacing.y);
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
					_verts[vert].y=0.5f*(_fallOff.Evaluate(fallOffT)-1);
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
		UpdateMeshData();
	}


	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.TransformPoint(_hitPos),0.1f);
		Gizmos.color=Color.blue;
		Gizmos.DrawWireSphere(transform.TransformPoint(_debugPos),_drawRadius);
	}
}
