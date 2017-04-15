using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// Poisson-disc sampling using Bridson's algorithm.
/// Adapted from Mike Bostock's Javascript source: http://bl.ocks.org/mbostock/19168c663618b7f07158
///
/// See here for more information about this algorithm:
///   http://devmag.org.za/2009/05/03/poisson-disk-sampling/
///   http://bl.ocks.org/mbostock/dbb02448b0f93e4c82c3
///
/// Usage:
///   PoissonDiscSampler sampler = new PoissonDiscSampler(10, 5, 0.3f);
///   foreach (Vector2 sample in sampler.Samples()) {
///       // ... do something, like instantiate an object at (sample.x, sample.y) for example:
///       Instantiate(someObject, new Vector3(sample.x, 0, sample.y), Quaternion.identity);
///   }
///
/// Author: Gregory Schlomoff (gregory.schlomoff@gmail.com)
/// Released in the public domain
public class PoissonDiscSampler {
    private const int _maximumSamples = 30;  // Maximum number of attempts before marking a sample as inactive.

    private readonly Rect _rect;
    //private readonly float _radiusSqrd;  // radius squared
    private readonly float _cellSize;
    private Disk[,] _grid;
    private List<Disk> _activeSamples = new List<Disk>();
    private Maps.Map _context;

    private float _minSize;
    private float _maxSize;

    /// Create a sampler with the following parameters:
    ///
    /// width:  each sample's x coordinate will be between [0, width]
    /// height: each sample's y coordinate will be between [0, height]
    /// radius: each sample will be at least `radius` units away from any other sample, and at most 2 * `radius`.
    public PoissonDiscSampler(float width, float height, float minSize, float maxSize, Maps.Map context)
    {
        RNG.Init();
        _rect = new Rect(0, 0, width, height);
        _context = context.Clone().Normalise();
        _minSize = minSize;
        _maxSize = maxSize;
        //_radiusSqrd = radius * radius;
        _cellSize = (minSize) / Mathf.Sqrt(2);
        _grid = new Disk[Mathf.CeilToInt(width / _cellSize),
                           Mathf.CeilToInt(height / _cellSize)];
    }

    /// Return a lazy sequence of samples. You typically want to call this in a foreach loop, like so:
    ///   foreach (Vector2 sample in sampler.Samples()) { ... }
    public IEnumerable<Disk> Samples()
    {
        // First sample is choosen randomly

        var firstPos = new Vector2(RNG.NextFloat() * _rect.width, RNG.NextFloat() * _rect.height);
        var firstSize = GetGrayscale(firstPos);

        yield return AddSample(new Disk(firstPos, firstSize));

        while (_activeSamples.Count > 0)
        {

            // Pick a random active sample
            var floater = RNG.NextFloat();
            int i = (int)floater * _activeSamples.Count;
            var currentSample = _activeSamples[i];

            // Try `k` random candidates between [radius, 2 * radius] from that sample.
            bool found = false;
            for (int j = 0; j < _maximumSamples; ++j)
            {

                float angle = 2 * Mathf.PI * RNG.NextFloat();
                float r = Mathf.Sqrt(RNG.NextFloat() * 3 * currentSample.RadiusSqr + currentSample.RadiusSqr); // See: http://stackoverflow.com/questions/9048095/create-random-number-within-an-annulus/9048443#9048443
                var candidatePos = currentSample.Position + r * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var candidateSize = GetGrayscale(candidatePos);

                var candidate = new Disk(candidatePos, candidateSize);



                // Accept candidates if it's inside the rect and farther than 2 * radius to any existing sample.
                if (_rect.Contains(candidatePos) && IsFarEnough(candidate))
                {
                    found = true;
                    yield return AddSample(candidate);
                    break;
                }
            }

            // If we couldn't find a valid candidate after k attempts, remove this sample from the active samples queue
            if (!found)
            {
                _activeSamples[i] = _activeSamples[_activeSamples.Count - 1];
                _activeSamples.RemoveAt(_activeSamples.Count - 1);
            }
        }
    }

    private bool IsFarEnough(Disk sample)
    {
        GridPos pos = new GridPos(sample.Position, _cellSize);

        int xmin = Mathf.Max(pos.x - 2, 0);
        int ymin = Mathf.Max(pos.y - 2, 0);
        int xmax = Mathf.Min(pos.x + 2, _grid.GetLength(0) - 1);
        int ymax = Mathf.Min(pos.y + 2, _grid.GetLength(1) - 1);

        for (int y = ymin; y <= ymax; y++)
        {
            for (int x = xmin; x <= xmax; x++)
            {
                Disk s = _grid[x, y];
                if (s.Position != Vector2.zero)
                {
                    Vector2 d = s.Position - sample.Position;
                    if (d.x * d.x + d.y * d.y < s.RadiusSqr)
                        return false;
                }
            }
        }

        return true;

        // Note: we use the zero vector to denote an unfilled cell in the grid. This means that if we were
        // to randomly pick (0, 0) as a sample, it would be ignored for the purposes of proximity-testing
        // and we might end up with another sample too close from (0, 0). This is a very minor issue.
    }

    /// Adds the sample to the active samples queue and the grid before returning it
    private Disk AddSample(Disk sample)
    {
        _activeSamples.Add(sample);
        GridPos pos = new GridPos(sample.Position, _cellSize);
        _grid[pos.x, pos.y] = sample;
        return sample;
    }

    private float GetGrayscale(Vector2 position)
    {
        var x = Mathf.InverseLerp(0, _rect.size.x, position.x);
        var y = Mathf.InverseLerp(0, _rect.size.y, position.y);

        var height = _context.BilinearSampleFromNormalisedVector2(new Vector2(x, y));
        return Mathf.Lerp(_minSize, _maxSize, height);
    }

    /// Helper struct to calculate the x and y indices of a sample in the grid
    private struct GridPos {
        public int x;
        public int y;

        public GridPos(Vector2 sample, float cellSize)
        {
            x = (int)(sample.x / cellSize);
            y = (int)(sample.y / cellSize);
        }
    }

    public struct Disk {
        public Vector2 Position { get; private set; }
        public float Radius { get; private set; }
        public float RadiusSqr { get; private set; }

        public Disk(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
            RadiusSqr = radius * radius;
        }
    }
}