# Regenerates every VFX texture. Needs: pip install numpy pillow
import numpy as np, os
from PIL import Image

out = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'Textures')
os.makedirs(out, exist_ok=True)

def save(name, alpha):
    o = np.zeros(alpha.shape + (4,), np.uint8)
    o[..., :3] = 255
    o[..., 3] = (np.clip(alpha, 0, 1) * 255).astype(np.uint8)
    Image.fromarray(o, 'RGBA').save(os.path.join(out, name + '.png'))

S = 256
y, x = np.mgrid[0:S, 0:S].astype(np.float32)
cx = (x - S / 2 + .5) / (S / 2); cy = (y - S / 2 + .5) / (S / 2)
r = np.hypot(cx, cy)
edge = np.clip((1 - r) / 0.25, 0, 1)

save('soft_circle', np.exp(-(r * 2.4) ** 2) * edge)
save('ring', np.exp(-((r - 0.7) / 0.09) ** 2) * edge)
save('spark', (np.exp(-(r * 6) ** 2) + 0.35 * np.exp(-(r * 2.5) ** 2)) * edge)

# Beam: tileable along U, soft falloff across V
W, H = 256, 128
u = np.linspace(0, 1, W, endpoint=False)[None, :]
v = np.linspace(0, 1, H)[:, None]
d = np.abs(v - 0.5) * 2
pattern = 0.72 + 0.2 * np.sin(2 * np.pi * 2 * u) + 0.08 * np.sin(2 * np.pi * 5 * u + 1.0)
save('beam', np.exp(-(d * 2.2) ** 2) * np.clip((1 - d) / 0.2, 0, 1) * pattern)

# Slash: crescent, thick edge toward +X (the swing direction)
outer = np.clip((0.95 - r) / 0.05, 0, 1)
inner = np.clip((np.hypot(cx + 0.32, cy) - 0.88) / 0.05, 0, 1)
lead = np.clip((cx + 0.95) / 1.4, 0, 1) ** 0.6
save('slash', outer * inner * (0.45 + 0.55 * lead))

# Explosion flipbook: 4x4 frames, 16 frames total
def fbm(size, seed):
    rng = np.random.default_rng(seed)
    acc = np.zeros((size, size), np.float32); amp = 1.0; tot = 0.0
    for g in (4, 8, 16, 32):
        a = (rng.random((g, g)) * 255).astype(np.uint8)
        im = np.asarray(Image.fromarray(a).resize((size, size), Image.BICUBIC), np.float32) / 255
        acc += im * amp; tot += amp; amp *= 0.5
    return acc / tot

N1, N2 = fbm(S, 1), fbm(S * 2, 2)
sheet = np.zeros((S * 4, S * 4), np.float32)
for i in range(16):
    t = i / 15
    R = 0.14 + 0.68 * (1 - (1 - t) ** 2.2)
    dd = r * (0.72 + 0.56 * N1)
    body = np.clip((R - dd) / 0.22, 0, 1)
    n2 = N2[i * 9:i * 9 + S, i * 13:i * 13 + S]
    holes = np.clip((n2 - t * 0.7) / 0.3 + 0.35, 0, 1)
    a = body * holes * (1 - t) ** 1.15 * 1.4
    core = np.clip(1 - r / (R * 0.75), 0, 1) ** 2 * (1 - t) ** 2.5
    a = np.clip(a + core, 0, 1) * edge
    row, col = divmod(i, 4)
    sheet[row * S:(row + 1) * S, col * S:(col + 1) * S] = a
save('explosion_sheet', sheet)
print('textures written to', os.path.abspath(out))
