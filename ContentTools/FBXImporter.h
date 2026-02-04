#pragma once
#include <fbxsdk.h>

#include "ToolsCommon.h"

namespace primal::tools
{
struct scene_data;
struct scene;
struct mesh;
struct geometry_import_settings;

class fbx_context
{
  public:
    fbx_context(const char *file, scene *scene, scene_data *scene_dat) : _scene(scene), _scene_data(scene_dat)
    {
        assert(file && _scene && _scene_data);

        if (initialize_fbx())
        {
            load_fbx_file(file);
        }
    }

    ~fbx_context()
    {
        _fbx_manager->Destroy();
        _fbx_manager->Destroy();

        ZeroMemory(this, sizeof(fbx_context));
    }

    void get_scene(FbxNode *root)
    {
        assert(is_valid());

        if (!root)
        {
            root = _fbx_scene->GetRootNode();

            if (!root)
                return;

            const s32 num_nodes{root->GetChildCount()};

            for (s32 i{0}; i < num_nodes; ++i)
            {
                FbxNode *node{root->GetChild(i)};

                if (!node)
                    continue;

                if (node->GetMesh())
                {
                    lod_group lod{};

                    get_mesh(node, lod.meshes);

                    if (lod.meshes.size())
                    {
                        lod.name = lod.meshes[0].name;

                        _scene->lod_groups.emplace_back(lod);
                    }
                }
                else if (node->GetLodGroup())
                {
                    get_lod_group(node);
                }
                else
                {
                    get_scene(node);
                }
            }
        }
    }

    constexpr bool is_valid() const
    {
        return _fbx_manager && _fbx_scene;
    }

    constexpr f32 scene_scale() const
    {
        return _scene_scale;
    }

  private:
    bool initialize_fbx();
    void load_fbx_file(const char *file);

    void get_mesh(FbxNode *node, utl::vector<mesh> &meshes);
    void get_lod_group(FbxNode *node);

    scene *_scene{nullptr};
    scene_data *_scene_data{nullptr};

    FbxManager *_fbx_manager{nullptr};
    FbxScene *_fbx_scene{nullptr};

    f32 _scene_scale{1.0f};
};
} // namespace primal::tools
