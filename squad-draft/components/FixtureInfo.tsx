import React, { useEffect, useState } from 'react';
import { fetchFixture } from '../api';

interface FixtureInfoProps {
  fixtureId: number;
}

const FixtureInfo: React.FC<FixtureInfoProps> = ({ fixtureId }) => {
  const [fixture, setFixture] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;
    const loadFixture = async () => {
      setLoading(true);
      const data = await fetchFixture(fixtureId);
      if (isMounted) {
        setFixture(data);
        setLoading(false);
      }
    };
    if (fixtureId) {
      loadFixture();
    }
    return () => {
      isMounted = false;
    };
  }, [fixtureId]);

  if (loading) {
    return <div className="text-xs text-slate-500 animate-pulse mt-1">Loading fixture info...</div>;
  }

  if (!fixture) {
    return null;
  }

  const { homeTeamName, awayTeamName, homeGoalCount, awayGoalCount } = fixture;

  const hasScore = homeGoalCount !== null && awayGoalCount !== null;

  return (
    <div className="bg-slate-900/50 rounded-lg p-3 mt-3 border border-slate-700/50 inline-block w-full md:w-auto">
      <div className="flex items-center gap-3 text-sm">
        <div className="font-semibold text-slate-300 flex-1 text-right">
          {homeTeamName}
        </div>
        
        {hasScore ? (
          <div className="bg-slate-800 px-3 py-1 rounded-md font-black text-yellow-400 min-w-[60px] text-center border border-slate-700">
            {homeGoalCount} - {awayGoalCount}
          </div>
        ) : (
          <div className="bg-slate-800 px-2 py-1 rounded-md font-bold text-slate-400 text-xs">
            VS
          </div>
        )}

        <div className="font-semibold text-slate-300 flex-1">
          {awayTeamName}
        </div>
      </div>
    </div>
  );
};

export default FixtureInfo;
