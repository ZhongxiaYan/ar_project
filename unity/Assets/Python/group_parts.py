import sys
import numpy as np

def load_obj(file_name):
    parts = {}
    with open(file_name, 'r+') as f:
        line = f.readline()
        curr_obj_name = None
        vertices = []
        curr_obj_vertices = set()

        while line:
            strs = line.rstrip().split(' ')
            if strs[0] == 'g':
                if curr_obj_name is not None:
                    parts[curr_obj_name] = np.array([vertices[i] for i in curr_obj_vertices])
                    curr_obj_vertices = set()
                curr_obj_name = strs[1]
            elif strs[0] == 'v':
                vertices.append([float(x) for x in strs[1:]])
            elif strs[0] == 'f':
                for fstr in strs[1:]:
                    curr_obj_vertices.add(int(fstr.split('/')[0]) - 1)
            line = f.readline()
        if curr_obj_name is not None:
            parts[curr_obj_name] = np.array([vertices[i] for i in curr_obj_vertices])
    return parts

def get_axis_stats(vertices):
    U, s, V_t = np.linalg.svd(vertices, full_matrices=False)
    projection = np.dot(vertices, V_t.T)
    return np.std(projection, axis=0)

def sort_categories(centered_parts):
    stat_to_part = {}
    for label, vertices in centered_parts.items():
        axis_stat = get_axis_stats(vertices)
        for stat, stat_list in stat_to_part.items():
            if np.linalg.norm(np.subtract(stat, axis_stat)) < 1e-3:
                stat_list.append(label)
                break
        else:
            stat_to_part[tuple(axis_stat)] = [label]
    return stat_to_part

def save_csv(file_name, groups):
    with open(file_name, 'w+') as f:
        for group in groups:
            f.write(','.join(group))
            f.write('\n')

parts = load_obj(sys.argv[1])
centered_parts = { name : part - np.mean(part, axis=0, keepdims=True) for name, part in parts.items() }

groups = sort_categories(centered_parts).values()

save_csv(sys.argv[2], groups)