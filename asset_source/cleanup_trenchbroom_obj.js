
function hashString(str) {
    let hash = 5381;
    const len = str.length;
    for (let i = 0; i < len; ++i) {
        let c = str.charCodeAt(i);
        hash = ((hash << 5) + hash) + c;
    }
    return hash;
}

function hashV2(v) {
    return hashString(`${v.x},${v.y}`);
}

function hashV3(v) {
    return hashString(`${v.x},${v.y},${v.z}`);
}

function createNewMaterial(matName) {
    return {
        name: matName,
        faces: []
    }
}

function createEmptyMesh(meshName) {
	return {
		name: meshName,
		materials: {}
	};
}

function createNewPrimitives() {
    return {
        verts: [],
		uvs: [],
		normals: []
    };
}

function extractOrderedListFromDict(dict) {
    return Object
        .keys(dict)
        .map(k => dict[k])
        .sort((a, b) => {
            if (a.i < b.i) { return -1 };
            if (a.i > b.i) { return 1; };
            return 0;
    });
}

////////////////////////////////////////////////////////
// writing
////////////////////////////////////////////////////////
function writeMesh(mesh, path) {
    console.log(`Writing mesh ${mesh.name} to ${path}`);
    // tally up primitives
    const prims = {
        vertsDict: {},
        uvsDict: {},
        normalsDict: {}
    };
    let nextVert = 1;
    let nextUV = 1;
    let nextNormal = 1;
    Object.keys(mesh.materials).forEach(key => {
        const mat = mesh.materials[key];
        mat.faces.forEach(face => {
            const numVerts = face.length;
            for (let i = 0; i < numVerts; ++i) {
                // console.log(face[i]);
                // const faceV = face[i].v;

                const vertHash = hashV3(face[i].v);
                if (!prims.vertsDict[vertHash]) {
                    let v = Object.assign({}, face[i].v);
                    v.i = nextVert;
                    nextVert += 1;
                    prims.vertsDict[vertHash] = v;
                }
                
                const uvHash = hashV2(face[i].vt);
                if (!prims.uvsDict[uvHash]) {
                    let uv = Object.assign({}, face[i].vt);
                    uv.i = nextUV;
                    nextUV += 1;
                    prims.uvsDict[uvHash] = uv;
                }

                const nHash = hashV3(face[i].vn);
                if (!prims.normalsDict[nHash]) {
                    let n = Object.assign({}, face[i].vn);
                    n.i = nextNormal;
                    nextNormal += 1;
                    prims.normalsDict[nHash] = n;
                }
            }
        });
    });
    // console.log(prims.vertsDict);
    // console.log(prims.uvsDict);
    // console.log(prims.normalsDict);

    let txt = `# vertices\n`;
    let vertsList = extractOrderedListFromDict(prims.vertsDict);
    let uvsList = extractOrderedListFromDict(prims.uvsDict);
    let normalsList = extractOrderedListFromDict(prims.normalsDict);
    vertsList.forEach(v => {
        txt += `v ${v.x} ${v.y} ${v.z}\n`;
    });
    txt += `\n# texture coordinates\n`;
    uvsList.forEach(uv => {
        txt += `vt ${uv.x} ${uv.y}\n`;
    });
    txt += `\n# face normals\n`;
    normalsList.forEach(n => {
        txt += `vn ${n.x} ${n.y} ${n.z}\n`;
    });
    txt += `\n# objects\n`;
    let bWroteObject = false;
    Object.keys(mesh.materials).map(k => mesh.materials[k]).forEach((mat, i) => {
        if (!bWroteObject) {
            txt += `\no ${mesh.name}\n`;
            bWroteObject = true
        }
        console.log(`Mat ${i}: ${mat.name}`);
        txt += `usemtl ${mat.name}\n`;
        mat.faces.forEach(face => {
            let faceTxt = "f";
            face.forEach(vert => {
                const vi = prims.vertsDict[hashV3(vert.v)].i;
                const vti = prims.uvsDict[hashV2(vert.vt)].i;
                const vni = prims.normalsDict[hashV3(vert.vn)].i;
                faceTxt += ` ${vi}/${vti}/${vni}`;
            });
            faceTxt += `\n`;
            txt += faceTxt;
        });
    });

    // console.log(vertsList);
    // console.log(uvsList);
    // console.log(normalsList);
    fs.writeFileSync(path, txt);
}

function writeMeshes(meshes, fileDir, fileNamePrefix) {
    const meshList = Object.keys(meshes).map(k => meshes[k]);
    console.log(`Writing ${meshList.length} meshes`);
    meshList.forEach(mesh => {
        let outputPath = fileNamePrefix;
        if (meshList.length > 1) {
            outputPath = `${fileNamePrefix}_${mesh.name}.obj`;
        }
		else if (!outputPath.endsWith(".obj"))
		{
			outputPath += ".obj";
		}
        if (fileDir !== "") {
            outputPath = `${fileDir}/${outputPath}`;
        }
        writeMesh(mesh, outputPath);
    });
}

////////////////////////////////////////////////////////
// reading
////////////////////////////////////////////////////////
function readMapLines(filePath) {
	const raw = fs.readFileSync(filePath, "utf-8");
	const lines = raw.split(/\r?\n/);
	console.log(`Read ${lines.length} lines from ${filePath}`);
	return lines;
}

function readEntityMeshes(inputPath) {
    const primitives = createNewPrimitives();
	const meshes = {};
    let mesh = null;
    let mat = null;
	const lines = readMapLines(inputPath);
	const numLines = lines.length;
	for (let i = 0; i < numLines; ++i) {
		// regex to replace any multiple spaces with a single space... otherwise split will fail
		//https://stackoverflow.com/questions/1981349/regex-to-replace-multiple-spaces-with-a-single-space
		const contractSpacesRegex = /  +/g;
		const line = lines[i].replace(contractSpacesRegex, ' ');
		const tokens = line.split(' ');
        // bounds checking is for squares.
        const numTokens = tokens.length;
		switch(tokens[0]) {
            ////////////////////////////////////////////////////////
            // primitives
            ////////////////////////////////////////////////////////
            case 'v': {
                const index = primitives.verts.length;
                primitives.verts.push({
                    i: index,
                    x: parseFloat(tokens[1]) * (1 / gScaleFactor),
				    y: parseFloat(tokens[2]) * (1 / gScaleFactor),
				    z: parseFloat(tokens[3]) * (1 / gScaleFactor)
                });
            } break;

            case 'vt': {
                const index = primitives.uvs.length;
                primitives.uvs.push({
                    i: index,
                    x: parseFloat(tokens[1]),
                    y: parseFloat(tokens[2])
                });
            } break;
            case 'vn': {
                const index = primitives.normals.length;
                primitives.normals.push({
                    i: index,
                    x: parseFloat(tokens[1]),
                    y: parseFloat(tokens[2]),
                    z: parseFloat(tokens[3])
                });
            } break;

            ////////////////////////////////////////////////////////
            // meshes
            ////////////////////////////////////////////////////////

            // add this face to the current material
            case 'f': {
                // if (mat.name === "skip") {
                //     continue;
                // }
                if (mat === null) {
                    continue;
                }
                // iterate through remaining tokens
                const face = [];
                mat.faces.push(face);
                for (let j = 1; j < numTokens; ++j) {
                    const vertexTokens = tokens[j].split('/');
                    if (vertexTokens.length === 3) {
                        const vi = parseInt(vertexTokens[0]) - 1;
						const vti = parseInt(vertexTokens[1]) - 1;
						const vni = parseInt(vertexTokens[2]) - 1;
						face.push({
							indexes: [ vi, vti, vni ],
							v: Object.assign({}, primitives.verts[vi]),
							vt: Object.assign({}, primitives.uvs[vti]),
							vn: Object.assign({}, primitives.normals[vni])
						});
                    }
					else if (vertexTokens.length === 4) {
						// k guessing that as two triangles this would be
						// 0, 1, 2, 0, 2, 3
						let vi = parseInt(vertexTokens[0]) - 1;
						let vti = parseInt(vertexTokens[1]) - 1;
						let vni = parseInt(vertexTokens[2]) - 1;
						face.push({
							indexes: [ vi, vti, vni ],
							v: Object.assign({}, primitives.verts[vi]),
							vt: Object.assign({}, primitives.uvs[vti]),
							vn: Object.assign({}, primitives.normals[vni])
						});
						
						vi = parseInt(vertexTokens[0]) - 1;
						vti = parseInt(vertexTokens[2]) - 1;
						vni = parseInt(vertexTokens[3]) - 1;
						face.push({
							indexes: [ vi, vti, vni ],
							v: Object.assign({}, primitives.verts[vi]),
							vt: Object.assign({}, primitives.uvs[vti]),
							vn: Object.assign({}, primitives.normals[vni])
						});

					}
					else {
						let msg = `Error in line '${line}': expected 3 or 4 tokens from face. Got ${vertexTokens.length}. `
						msg += `Tokens: '${vertexTokens.join(", ")}' `;
						// throw `sigh`;
						throw msg;
					}
                    // object file primitive indexes start from 1 not 0
                    
                }
            } break;

			case 'o': {
				const name = tokens[1].split('_')[0];
                mesh = meshes[name];
				if (typeof(mesh) === "undefined") {
                    mesh = createEmptyMesh(name);
					meshes[name] = mesh;
                    // console.log(`Created mesh ${name}`);
                }
                // else {
                //     console.log(`Found mesh ${mesh.name}`);
                // }
			} break;

            case 'usemtl': {
                const split = tokens[1].split('/');
                const name = split[split.length - 1];
                if (name === "skip") {
                    mat = null;
                    continue;
                }
                if (!mesh) {
                    throw 'No mesh for usemtl';
                }
                mat = mesh.materials[name];
                if (typeof(mat) === "undefined") {
                    // console.log(`Created mat ${name}`);
                    mat = createNewMaterial(name);
                    mesh.materials[name] = mat;
                }
                // else {
                //     console.log(`Found mat ${mat.name}`);
                // }
            } break;
		}
	}
	
    console.log(`Read ${Object.keys(meshes).length} meshes`);
    return meshes;
}

function dumpMeshes(meshes) {
    // console.log(JSON.stringify(meshes, null, 4));
    const keys = Object.keys(meshes);
	keys.forEach(k => {
        const mesh = meshes[k];
		console.log(`${mesh.name}: ${hashString(k)}`);
        const matKeys = Object.keys(mesh.materials);
        matKeys.forEach(matKey => {
            const mat = mesh.materials[matKey];
            console.log(`\tMat ${mat.name} has ${mat.faces.length} faces`);
        });
	});
}

//////////////////////////////////////////////
// run
const gDebugMeshRead = false
const gScaleFactor = 16;
const fs = require("fs");
const gInputPath = process.argv.length >= 3 ? process.argv[2] : "test_map_chunk.obj";
const gOutputDir = process.argv.length >= 4 ? process.argv[3] : "";
const gOutputFilePrefix = process.argv.length >= 5 ? process.argv[4] : "output";
console.log(`Input ${gInputPath}, output dir ${gOutputDir} file prefix ${gOutputFilePrefix}`);
const meshes = readEntityMeshes(gInputPath);

if (gDebugMeshRead) {
    // for debugging - see the data the extraction will generate:
    fs.writeFileSync("meshes_raw.json", JSON.stringify(meshes, null, 4))
    return
}
writeMeshes(meshes, gOutputDir, gOutputFilePrefix);
