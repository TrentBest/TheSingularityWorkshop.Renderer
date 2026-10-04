# Parallax and Observer Geometry

## The mountain problem

A renderer should not make a distant mountain disappear merely because it is far away. If the mountain is physically present, visible, and large enough to affect the observer's view, distance should primarily change how it is represented and how quickly its apparent state changes.

This is different from hiding it with fog.

## Perspective projection

For a pinhole camera, a horizontal world coordinate X at positive depth Z projects to:

    u = f * X / Z

where u is projected horizontal coordinate and f is focal length in image-coordinate units.

A feature of physical width W at distance D has small-angle angular size approximately:

    angular_size ≈ W / D

The exact angular extent can use atan2. The important point is that a distant object can have small but non-zero projected consequence.

## Parallax

Move the observer sideways by ΔC. For a stationary point at depth Z:

    Δu = -f * ΔC / Z

The sign depends on the coordinate convention. The magnitude is:

    |Δu| = f * |ΔC| / Z

For two depths, relative parallax is:

    |Δu_relative| = f * |ΔC| * |1/Z_near - 1/Z_far|

This is the mathematical relationship behind the experience of watching nearby objects move rapidly across the view while mountains barely move.

## Concrete mountain example

Suppose the display is 1920 pixels wide with a 90 degree horizontal field of view, the observer moves laterally by 10 m, a near object is 10 m away, and a mountain is 300 km away.

The focal length in pixels is:

    f = 1920 / (2 * tan(90° / 2)) = 960 pixels

The near object moves approximately:

    960 * 10 / 10 = 960 pixels

The mountain moves approximately:

    960 * 10 / 300000 = 0.032 pixels

The relative motion is approximately 959.968 pixels.

Nothing was hidden and no fog was required. The mountain simply has extremely low parallax relative to the observer.

## Parallax velocity

If lateral observer velocity is v, then:

    du/dt = -f * v / Z

and relative parallax velocity is:

    |du_relative/dt| = f * |v| * |1/Z_near - 1/Z_far|

This gives Event Horizon policy a temporal signal. A distant mountain can remain visible continuously while requiring much less frequent state evaluation because its projected position changes slowly.

## World population versus observable workload

Let N_world be total entities and N_visible(observer, t) be entities with current or predicted observable consequence.

The renderer should not assume work is proportional to N_world. A first-order observer-relative model is:

    work(observer, t) ≈ sum C(entity_i, observer, policy)

over entities whose current or predicted consequence justifies evaluation.

For cohorts:

    C_total/s ≈ sum N_i * C_i * f_i

where each cohort has population N_i, per-evaluation cost C_i, and cadence f_i.

This is the mathematical opening for scaling worlds beyond the number of entities that deserve expensive computation at one instant.

## Metrics we should collect

Graphics performance guidance consistently separates frame time and CPU/GPU stages and recommends identifying the actual bottleneck before optimizing. The Renderer should therefore track:

- observer geometry time: projection, angular size, parallax, horizon selection
- CPU time: selection, semantic lookup, FSM scheduling, representation construction
- submission time: command generation, draw calls, state changes
- GPU time: frame time and individual passes
- geometry workload: visible triangles and primitives
- pixel workload: visible fragments or pixels where measurable
- memory: resident, transient, and streamed representation bytes
- streaming: bytes moved and residency churn
- outcome: frame-time variance, latency, visible information preserved, and work avoided

NVIDIA's graphics guidance describes bottleneck identification as the first step in optimization, while Microsoft profiling guidance exposes frame timing and GPU usage separately. Those practices fit the Renderer because they let us compare predicted cost against measured cost instead of reducing performance to FPS alone.

## The hypothesis

The Renderer is not claiming that infinitely many entities can be rendered.

The testable claim is:

> As world population increases, observer-relative computational work can scale primarily with observable consequence rather than raw world population, while preserving physically meaningful distant visibility and motion.

That can be tested with synthetic worlds of increasing population, controlled observer motion, and identical observation policies.

## Next mathematical layers

    world coordinates
        -> observer transform
        -> perspective projection
        -> projected size
        -> projected motion / parallax
        -> visibility
        -> observable significance
        -> Event Horizon selection
        -> FSM cohort / cadence
        -> measured computational cost

The goal is to compare naive work against observer-relative work without using fog or arbitrary disappearance as a substitute for rendering.